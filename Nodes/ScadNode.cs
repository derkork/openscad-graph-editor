using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.PortContainer;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class ScadNode : GraphNode
    {
        [Signal]
        public delegate void Changed();

        public abstract string NodeTitle { get; }

        public abstract string NodeDescription { get; }


        protected readonly List<PortDefinition> InputPorts = new List<PortDefinition>();
        protected readonly List<PortDefinition> OutputPorts = new List<PortDefinition>();

        private readonly Dictionary<int, IScadLiteralWidget> _inputLiteralWidgets =
            new Dictionary<int, IScadLiteralWidget>();

        private readonly Dictionary<int, IScadLiteralWidget> _outputLiteralWidgets =
            new Dictionary<int, IScadLiteralWidget>();


        public void PortConnected(int port, bool isLeft)
        {
            if (isLeft)
            {
                if (_inputLiteralWidgets.TryGetValue(port, out var widget))
                {
                    widget.SetEnabled(false);
                }
            }
        }

        public void PortDisconnected(int port, bool isLeft)
        {
            if (isLeft)
            {
                if (_inputLiteralWidgets.TryGetValue(port, out var widget))
                {
                    widget.SetEnabled(true);
                }
            }
        }
        
        
        
        public bool HasInputOfType(PortType type)
        {
            return InputPorts.Any(it => it.PortType == type);
        }

        public bool HasOutputOfType(PortType type)
        {
            return OutputPorts.Any(it => it.PortType == type);
        }
        
        public int GetFirstInputPortOfType(PortType portType)
        {
            return InputPorts.FindIndex(it => it.PortType == portType);
        }

        public int GetFirstOutputPortOfType(PortType portType)
        {
            return OutputPorts.FindIndex(it => it.PortType == portType);
        }

        public PortType GetInputPortType(int index)
        {
            return InputPorts[index].PortType;
        }

        public PortType GetOutputPortType(int index)
        {
            return OutputPorts[index].PortType;
        }

        public abstract string Render(ScadContext scadContext);

        protected string RenderInput(ScadContext scadContext, int index)
        {
            // if we have a node connected, render the node
            if (scadContext.TryGetInputNode(this, index, out var node))
            {
                return node.Render(scadContext);
            }

            // try rendering the widget
            if (_inputLiteralWidgets.TryGetValue(index, out var widget))
            {
                return widget.Value;
            }

            // otherwise return nothing
            return "";
        }

        protected string RenderOutput(ScadContext scadContext, int index)
        {
            // if we have a node connected, render the node
            if (scadContext.TryGetOutputNode(this, index, out var node))
            {
                return node.Render(scadContext);
            }

            // try rendering the widget
            if (_outputLiteralWidgets.TryGetValue(index, out var widget))
            {
                return widget.Value;
            }

            // otherwise return nothing
            return "";
        }


        public override void _Ready()
        {
            Title = NodeTitle;
            HintTooltip = NodeDescription;
            RectMinSize = new Vector2(200, 120);

            ClearAllSlots();
            foreach (var childNode in this.GetChildNodes())
            {
                childNode.RemoveAndFree();
            }

            var maxPorts = Mathf.Max(InputPorts.Count, OutputPorts.Count);

            var idx = 0;
            while (idx < maxPorts)
            {
                var container = new HBoxContainer();
                AddChild(container);

                if (InputPorts.Count > idx)
                {
                    BuildPort(container, idx, InputPorts[idx], true);
                }

                if (OutputPorts.Count > idx)
                {
                    BuildPort(container, idx, OutputPorts[idx], false);
                }

                idx++;
            }
        }

        private void BuildPort(Container container, int idx, PortDefinition portDefinition, bool isLeft)
        {
            if (isLeft)
            {
                SetSlotEnabledLeft(idx, true);
                SetSlotColorLeft(idx, ColorFor(portDefinition.PortType));
                SetSlotTypeLeft(idx, (int) portDefinition.PortType);
            }
            else
            {
                SetSlotEnabledRight(idx, true);
                SetSlotColorRight(idx, ColorFor(portDefinition.PortType));
                SetSlotTypeRight(idx, (int) portDefinition.PortType);
            }

            IScadLiteralWidget literalWidget = null;
            if (portDefinition.AllowLiteral)
            {
                switch (portDefinition.PortType)
                {
                    case PortType.Boolean:
                        literalWidget = Prefabs.New<BooleanEdit>();
                        break;
                    case PortType.Number:
                        literalWidget = Prefabs.New<NumberEdit>();
                        break;
                    case PortType.String:
                        literalWidget = Prefabs.New<StringEdit>();
                        break;
                    case PortType.Vector3:
                        literalWidget = Prefabs.New<Vector3Edit>();
                        break;
                    case PortType.Array:
                        throw new ArgumentException("Not implemented!");
                    case PortType.Range:
                        throw new ArgumentException("Not implemented!");
                    case PortType.Flow:
                    case PortType.Any:
                        throw new ArgumentException(
                            $"Port type {portDefinition.PortType} cannot have a literal input.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (isLeft)
                {
                    _inputLiteralWidgets[idx] = literalWidget;
                }
                else
                {
                    _outputLiteralWidgets[idx] = literalWidget;
                }

                literalWidget.ConnectChanged().To(this, nameof(NotifyChanged));
            }

            // TODO: this is sort of a hack, better clone this stuff somewhere
            var portContainer = Prefabs.InstantiateFromScene<PortContainer>();
            portContainer._Ready();
            portContainer.Setup(isLeft, portDefinition.Name, (Control) literalWidget);
            portContainer.MoveToNewParent(container);
        }


        private Color ColorFor(PortType portType)
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
                    return new Color(.2f, 0.2f, 1f);
                case PortType.String:
                    return new Color(1f, 1f, 0f);
                case PortType.Range:
                    return new Color(.2f, 0.2f, 1f);
                case PortType.Any:
                    return new Color(0, 0f, 0f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }

        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }

        private void NotifyChanged()
        {
            EmitSignal(nameof(Changed));
        }
    }
}