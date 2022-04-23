using System;
using System.Collections.Generic;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class ScadNodeWidget : GraphNode
    {
        [Signal]
        public delegate void Changed(bool codeChange);

        public event Action<ScadNode, Vector2> PositionChanged;
        

        private bool _offsetChangePending;
        private bool _initializing;
        
        public override void _Ready()
        {
            this.Connect("offset_changed")
                .To(this, nameof(OnOffsetChanged));
        }

        private void OnOffsetChanged()
        {
            // we get offset changes continuously as the mouse is dragged but
            // we only want to update the code once per drag. Therefore we just
            // note that the node offset has changed and wait for a mouse release to
            // actually send the event.
            _offsetChangePending = !_initializing;
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (!_offsetChangePending || !(inputEvent is InputEventMouseButton mouseButtonEvent))
            {
                return;
            }

            if (mouseButtonEvent.IsPressed() || mouseButtonEvent.ButtonIndex != (int) ButtonList.Left)
            {
                return;
            }
            
            PositionChanged?.Invoke(BoundNode, Offset);   
            _offsetChangePending = false;
        }

        public ScadNode BoundNode { get; protected set; }

        public virtual void BindTo(IScadGraph graph, ScadNode node)
        {
            // setting node values may trigger change events which we don't want to
            // therefore we set _initializing to true to prevent these events from
            // being observed.
            _initializing = true;
            
            BoundNode = node;
            Title = node.NodeTitle;
            HintTooltip = node.NodeDescription;
            RectMinSize = new Vector2(200, 120);
            Offset = node.Offset;

            var maxPorts = Mathf.Max(node.InputPortCount, node.OutputPortCount);

            var idx = 0;
            while (idx < maxPorts)
            {
                var container = new HBoxContainer();
                AddChild(container);

                if (node.InputPortCount > idx)
                {
                    BuildPort(container, idx, node.GetInputPortDefinition(idx), true, graph, node);
                }

                if (node.OutputPortCount > idx)
                {
                    BuildPort(container, idx, node.GetOutputPortDefinition(idx), false, graph, node);
                }

                idx++;
            }
            QueueSort();
            
            // re-enable event observing
            _initializing = false;
        }

        private void BuildPort(Container container, int idx, PortDefinition portDefinition, bool isLeft, IScadGraph graph, ScadNode node)
        {
            var connectorPortType = portDefinition.AutoCoerce ? PortType.Any : portDefinition.PortType;
            if (isLeft)
            {
                SetSlotEnabledLeft(idx, true);
                SetSlotColorLeft(idx, ColorFor(connectorPortType));
                SetSlotTypeLeft(idx, (int) connectorPortType);
            }
            else
            {
                SetSlotEnabledRight(idx, true);
                SetSlotColorRight(idx, ColorFor(connectorPortType));
                SetSlotTypeRight(idx, (int) connectorPortType);
            }

            
            var isConnected = isLeft ? graph.IsInputConnected(node, idx) : graph.IsOutputConnected(node, idx);

            
            IScadLiteralWidget literalWidget = null;

            var literal = isLeft ? node.GetInputLiteral(idx) : node.GetOutputLiteral(idx);
            switch (literal)
            {
                case BooleanLiteral booleanLiteral:
                    var booleanEdit = Prefabs.New<BooleanEdit>();
                    booleanEdit.BindTo(booleanLiteral);
                    literalWidget = booleanEdit;
                    break;

                case NumberLiteral numberLiteral:
                    var numberEdit = Prefabs.New<NumberEdit>();
                    numberEdit.BindTo(numberLiteral);
                    literalWidget = numberEdit;
                    break;

                case StringLiteral stringLiteral:
                    var stringEdit = Prefabs.New<StringEdit>();
                    stringEdit.BindTo(stringLiteral);
                    literalWidget = stringEdit;
                    break;

                case Vector3Literal vector3Literal:
                    var vector3Edit = Prefabs.New<Vector3Edit>();
                    vector3Edit.BindTo(vector3Literal);
                    literalWidget = vector3Edit;
                    break;
            }

            if (literalWidget != null)
            {
                literalWidget.SetEnabled(!isConnected);
                literalWidget.ConnectChanged()
                    .WithBinds(true)
                    .To(this, nameof(NotifyChanged));
            }

            var portContainer = Prefabs.InstantiateFromScene<PortContainer.PortContainer>();
            portContainer.MoveToNewParent(container);
            portContainer.Setup(isLeft, portDefinition.Name, (Control) literalWidget);
        }


        protected Color ColorFor(PortType portType)
        {
            switch (portType)
            {
                case PortType.Flow:
                    return new Color(1, 1, 1);
                case PortType.Boolean:
                    return new Color(1, 0.4f, 0.4f);
                case PortType.Number:
                    return new Color(1, 0.8f, 0.4f);
                case PortType.Vector3:
                    return new Color(.8f, 0.8f, 1f);
                case PortType.Array:
                    return new Color(.5f, 0.5f, 1f);
                case PortType.String:
                    return new Color(1f, 1f, 0f);
                case PortType.Any:
                    return new Color(1, 0f, 1f);
                case PortType.Reroute:
                    return new Color(0, 0.8f, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }

        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }

        private void NotifyChanged(bool codeChange)
        {
            EmitSignal(nameof(Changed), codeChange);
        }
    }
}