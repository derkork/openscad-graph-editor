using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class ScadNodeWidget : GraphNode
    {
        public event Action<Vector2> PositionChanged;
        public event Action<PortId, object> LiteralValueChanged;
        public event Action<PortId, bool> LiteralToggled;
        

        private readonly Dictionary<PortId, IScadLiteralWidget> _literalWidgets = new Dictionary<PortId, IScadLiteralWidget>();
        
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
            // if we have an offset change pending and the mouse was released
            if (!_offsetChangePending || !(inputEvent is InputEventMouseButton mouseButtonEvent))
            {
                return;
            }

            if (mouseButtonEvent.IsPressed() || mouseButtonEvent.ButtonIndex != (int) ButtonList.Left)
            {
                return;
            }
            
            // notify about a position change.
            PositionChanged?.Invoke(Offset);   
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
            Offset = node.Offset;

            var maxPorts = Mathf.Max(node.InputPortCount, node.OutputPortCount);
            
            var existingContainers = this.GetChildNodes<HBoxContainer>().ToList();
            
            var idx = 0;
            while (idx < maxPorts)
            {
                PortContainer.PortContainer left;
                PortContainer.PortContainer right;
                
                if (existingContainers.Count <= idx)
                {
                    // make a new container
                    var parent = new HBoxContainer();
                    parent.MoveToNewParent(this);
                    left  = Prefabs.InstantiateFromScene<PortContainer.PortContainer>();
                    left.MoveToNewParent(parent);
                    right = Prefabs.InstantiateFromScene<PortContainer.PortContainer>();
                    right.MoveToNewParent(parent);
                }
                else
                {
                    left = existingContainers[idx].GetChild<PortContainer.PortContainer>(0);
                    right = existingContainers[idx].GetChild<PortContainer.PortContainer>(1);
                }

                if (node.InputPortCount > idx)
                {
                    BuildPort(left,  graph, node, PortId.Input(idx));
                }
                else
                {
                    left.Clear();
                }

                if (node.OutputPortCount > idx)
                {
                    BuildPort(right, graph, node, PortId.Output(idx));
                }
                else
                {
                    right.Clear();
                }

                idx++;
            }
            
            // remove any remaining containers
            while (existingContainers.Count > idx)
            {
                existingContainers[idx].RemoveAndFree();
            }
            
            // set to minimum size. Needs to be called
            // deferred as it doesn't seem to have any effect when being called in the same
            // frame as when the widget is created.
            CallDeferred(nameof(Minimize));
            
            // re-enable event observing
            _initializing = false;
        }

        private void Minimize()
        {
            SetSize(new Vector2(1,1));
            QueueSort();
        }

        private void BuildPort(PortContainer.PortContainer container, IScadGraph graph, ScadNode node, PortId port)
        {
            var portDefinition = node.GetPortDefinition(port);
            var idx = port.Port;
            
            var connectorPortType = portDefinition.AutoCoerce ? PortType.Any : portDefinition.PortType;
            if (port.IsInput)
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


            var isConnected = graph.IsPortConnected(node, port);

            
            IScadLiteralWidget literalWidget = null;

            _literalWidgets.TryGetValue(port, out var existingWidget);
            
            if (node.TryGetLiteral(port, out var literal))
            {
                
                switch (literal)
                {
                    case BooleanLiteral booleanLiteral:
                        if (!(existingWidget is BooleanEdit booleanEdit))
                        {
                            booleanEdit = Prefabs.New<BooleanEdit>();
                        }

                        booleanEdit.BindTo(booleanLiteral);
                        literalWidget = booleanEdit;
                        break;

                    case NumberLiteral numberLiteral:
                        if (!(existingWidget is NumberEdit numberEdit))
                        {
                            numberEdit = Prefabs.New<NumberEdit>();
                        }
                        numberEdit.BindTo(numberLiteral);
                        literalWidget = numberEdit;
                        break;

                    case StringLiteral stringLiteral:
                        if (!(existingWidget is StringEdit stringEdit))
                        {
                            stringEdit = Prefabs.New<StringEdit>();
                        }
                        stringEdit.BindTo(stringLiteral);
                        literalWidget = stringEdit;
                        break;

                    case Vector3Literal vector3Literal:
                        if (!(existingWidget is Vector3Edit vector3Edit))
                        {
                            vector3Edit = Prefabs.New<Vector3Edit>();
                        }
                        vector3Edit.BindTo(vector3Literal);
                        literalWidget = vector3Edit;
                        break;
                }
            }

            if (existingWidget != null && existingWidget != literalWidget)
            {
                GD.Print("Widget mismatch");
                // we replaced the widget with something else, so delete the existing widget
                ((Node)existingWidget).RemoveAndFree();
                _literalWidgets.Remove(port);
            }

            if (literalWidget != null)
            {
                literalWidget.SetEnabled(!isConnected);
                // only wire the events if we have an new widget.
                if (existingWidget != literalWidget)
                {
                    literalWidget.LiteralToggled += (value) => LiteralToggled?.Invoke(port, value);
                    literalWidget.LiteralValueChanged += (value) => LiteralValueChanged?.Invoke(port, value);
                }
                _literalWidgets[port] = literalWidget;
            }

            container.Setup(port.IsInput, portDefinition.Name, (Control) literalWidget);
        }


        protected static Color ColorFor(PortType portType)
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
    }
}