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


        public virtual void SaveInto(SavedNode node)
        {
            node.Id = Name;
            node.Script = ((CSharpScript) GetScript()).ResourcePath;
            node.Position = Offset;
            InputPorts
                .Indices()
                .Where(i => _inputLiteralWidgets.ContainsKey(i))
                .ForAll(i => node.SetData($"input_widget_value.{i}", _inputLiteralWidgets[i].SerializedValue));
            OutputPorts
                .Indices()
                .Where(i => _outputLiteralWidgets.ContainsKey(i))
                .ForAll(i => node.SetData($"output_widget_value.{i}", _outputLiteralWidgets[i].SerializedValue));
        }

        public virtual void PrepareForLoad(SavedNode node)
        {
        }
        
        public virtual void LoadFrom(SavedNode node)
        {
            Name = node.Id;
            Offset = node.Position;
            InputPorts
                .Indices()
                .Where(i => _inputLiteralWidgets.ContainsKey(i))
                .ForAll(i => _inputLiteralWidgets[i].SerializedValue = node.GetData($"input_widget_value.{i}"));
            OutputPorts
                .Indices()
                .Where(i => _outputLiteralWidgets.ContainsKey(i))
                .ForAll(i => _outputLiteralWidgets[i].SerializedValue = node.GetData($"output_widget_value.{i}"));
        }

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


        public bool HasInputThatCanConnect(PortType type)
        {
            return InputPorts.Any(it => it.PortType.CanConnect(type) || (type != PortType.Flow && it.AutoCoerce));
        }

        public bool HasOutputThatCanConnect(PortType type)
        {
            return OutputPorts.Any(it => it.PortType.CanConnect(type));
        }

        public int GetFirstInputThatCanConnect(PortType type)
        {
            return InputPorts.FindIndex(it => it.PortType.CanConnect(type) || (type != PortType.Flow && it.AutoCoerce));
        }

        public int GetFirstOutputThatCanConnect(PortType type)
        {
            return OutputPorts.FindIndex(it => it.PortType.CanConnect(type));
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
                return widget.RenderedValue;
            }

            // otherwise return nothing
            return "";
        }

        protected string RenderOutput(ScadContext scadContext, int index)
        {
            // if we have a flow node connected render this.
            if (GetOutputPortType(index) == PortType.Flow)
            {
                if (scadContext.TryGetOutputNode(this, index, out var node))
                {
                    return node.Render(scadContext);
                }
            }

            // try rendering the widget
            if (_outputLiteralWidgets.TryGetValue(index, out var widget))
            {
                return widget.RenderedValue;
            }

            // otherwise return nothing
            return "";
        }


        public override void _Ready()
        {
            Title = NodeTitle;
            HintTooltip = NodeDescription;
            RectMinSize = new Vector2(200, 120);

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

            var portContainer = Prefabs.InstantiateFromScene<PortContainer>();
            portContainer.MoveToNewParent(container);
            portContainer.Setup(isLeft, portDefinition.Name, (Control) literalWidget);
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
                    return new Color(1, 0f, 1f);
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