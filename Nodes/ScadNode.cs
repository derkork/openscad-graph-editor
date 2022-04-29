using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class ScadNode
    {
        public abstract string NodeTitle { get; }

        public abstract string NodeDescription { get; }

        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public Vector2 Offset { get; set; }

        public int InputPortCount => InputPorts.Count;
        public int OutputPortCount => OutputPorts.Count;

        protected readonly List<PortDefinition> InputPorts = new List<PortDefinition>();
        protected readonly List<PortDefinition> OutputPorts = new List<PortDefinition>();

        private readonly Dictionary<int, IScadLiteral> _inputLiterals =
            new Dictionary<int, IScadLiteral>();

        private readonly Dictionary<int, IScadLiteral> _outputLiterals =
            new Dictionary<int, IScadLiteral>();


        /// <summary>
        /// Custom node attributes.
        /// </summary>
        private readonly Dictionary<string, string> _customAttributes
            = new Dictionary<string, string>();


        public bool TryGetLiteral(PortId port, out IScadLiteral result)
        {
            if (port.IsInput)
            {
                return _inputLiterals.TryGetValue(port.Port, out result);
            }
            else
            {
                return _outputLiterals.TryGetValue(port.Port, out result);
            }
        }

        public PortDefinition GetPortDefinition(PortId port)
        {
            return port.IsInput ? InputPorts[port.Port] : OutputPorts[port.Port];
        }


        public void DropPortLiteral(PortId port)
        {
            if (port.IsInput)
            {
                _inputLiterals.Remove(port.Port);
            }
            else
            {
                _outputLiterals.Remove(port.Port);
            }
        }

        public void SwapInputLiterals(int index0, int index1)
        {
            var hasFirst = _inputLiterals.TryGetValue(index0, out var literal0);
            var hasSecond = _inputLiterals.TryGetValue(index1, out var literal1);

            if (hasFirst)
            {
                _inputLiterals[index1] = literal0;
            }
            else
            {
                _inputLiterals.Remove(index1);
            }

            if (hasSecond)
            {
                _inputLiterals[index0] = literal1;
            }
            else
            {
                _inputLiterals.Remove(index0);
            }
        }

        public void SwapOutputLiterals(int index0, int index1)
        {
            var hasFirst = _outputLiterals.TryGetValue(index0, out var literal0);
            var hasSecond = _outputLiterals.TryGetValue(index1, out var literal1);

            if (hasFirst)
            {
                _outputLiterals[index1] = literal0;
            }
            else
            {
                _outputLiterals.Remove(index1);
            }

            if (hasSecond)
            {
                _outputLiterals[index0] = literal1;
            }
            else
            {
                _outputLiterals.Remove(index0);
            }
        }

        public virtual void SaveInto(SavedNode node)
        {
            node.Id = Id;
            node.Type = GetType().FullName;
            node.Position = Offset;
            InputPorts
                .Indices()
                .Where(i => _inputLiterals.ContainsKey(i))
                .ForAll(i =>
                {
                    node.SetData($"input_literal_value.{i}", _inputLiterals[i].SerializedValue);
                    node.SetData($"input_literal_set.{i}", _inputLiterals[i].IsSet);
                });
            OutputPorts
                .Indices()
                .Where(i => _outputLiterals.ContainsKey(i))
                .ForAll(i =>
                {
                    node.SetData($"output_literal_value.{i}", _outputLiterals[i].SerializedValue);
                    node.SetData($"output_literal_set.{i}", _outputLiterals[i].IsSet);
                });

            _customAttributes.ForAll(it => { node.SetData($"custom_attribute.{it.Key}", it.Value); });
        }


        public void PrepareLiteralsFromPortDefinitions()
        {
            var maxPorts = Mathf.Max(InputPorts.Count, OutputPorts.Count);
            var idx = 0;
            while (idx < maxPorts)
            {
                if (InputPorts.Count > idx)
                {
                    BuildPortLiteral(PortId.Input(idx));
                }

                if (OutputPorts.Count > idx)
                {
                    BuildPortLiteral(PortId.Output(idx));
                }

                idx++;
            }
        }

        /// <summary>
        /// Loading Phase 1: Called to restore the port definitions from the saved node.
        /// </summary>
        public virtual void RestorePortDefinitions(SavedNode node, IReferenceResolver resolver)
        {
            _customAttributes.Clear();
            // restore custom attributes
            node.StoredData
                .Where(it => it.Key.StartsWith("custom_attribute."))
                .ForAll(it => _customAttributes[it.Key.Substring("custom_attribute.".Length)] = it.Value);
        }

        /// <summary>
        /// Loading Phase 2: Called to restore the literal structure from the saved node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="resolver"></param>
        public virtual void RestoreLiteralStructures(SavedNode node, IReferenceResolver resolver)
        {
            PrepareLiteralsFromPortDefinitions();
        }

        /// <summary>
        /// Loading Phase 3: Called to restore the literal values from the saved node. 
        /// </summary>
        public virtual void RestoreLiteralValues(SavedNode node, IReferenceResolver referenceResolver)
        {
            Id = node.Id;
            Offset = node.Position;
            InputPorts
                .Indices()
                .Where(i => _inputLiterals.ContainsKey(i))
                .ForAll(i =>
                {
                    _inputLiterals[i].SerializedValue = node.GetData($"input_literal_value.{i}");
                    _inputLiterals[i].IsSet = node.GetDataBool($"input_literal_set.{i}", true);
                });
            OutputPorts
                .Indices()
                .Where(i => _outputLiterals.ContainsKey(i))
                .ForAll(i =>
                {
                    _outputLiterals[i].SerializedValue = node.GetData($"output_literal_value.{i}");
                    _outputLiterals[i].IsSet = node.GetDataBool($"output_literal_set.{i}", true);
                });
        }


        public PortType GetPortType(PortId port)
        {
            return port.IsInput ? InputPorts[port.Port].PortType : OutputPorts[port.Port].PortType;
        }

        public abstract string Render(IScadGraph context);

        private string RenderPort(IScadGraph context, PortId port)
        {
            if (port.IsInput)
            {
                // if we have a node connected, render the node
                if (context.TryGetConnectedNode(this, port, out var node, out var otherPort))
                {
                    // there is a specialty for input ports, they may be connected to a multi expression output
                    // node, so we need to instruct the other node to render the correct output port
                    if (node is IHaveMultipleExpressionOutputs multiNode)
                    {
                        return multiNode.RenderExpressionOutput(context, otherPort.Port);
                    }

                    return node.Render(context);
                }
            }
            else
            {
                // for output nodes we only render connected ports if the port type is "Flow".
                if (GetPortType(port) == PortType.Flow)
                {
                    if (context.TryGetConnectedNode(this, port, out var node, out _))
                    {
                        var rendered = node.Render(context);
                        if (rendered.Empty())
                        {
                            return rendered; // if the node is empty, no point adding any modifiers to it.
                        }
                        var renderModifier = node.BuildRenderModifier();
                        if (!renderModifier.Empty())
                        {
                            return string.Format(renderModifier, rendered);
                        }

                        return rendered;
                    }
                }
            }

            // try rendering the literal
            if (TryGetLiteral(port, out var literal) && literal.IsSet)
            {
                return literal.RenderedValue;
            }

            // otherwise return nothing
            return "";
        }

        private string BuildRenderModifier()
        {
            var effectiveModifiers = this.GetModifiers();
            this.TryGetColorModifier(out var effectiveColor);

            // we also need to set a render modifier so the modifiers are rendered
            var renderModifier = "";
            if (effectiveModifiers.HasFlag(ScadNodeModifier.Debug))
            {
                renderModifier = "# {0}";
            }

            if (effectiveModifiers.HasFlag(ScadNodeModifier.Background))
            {
                renderModifier = "% {0}";
            }

            if (effectiveModifiers.HasFlag(ScadNodeModifier.Root))
            {
                renderModifier = "! {0}";
            }

            if (effectiveModifiers.HasFlag(ScadNodeModifier.Disable))
            {
                renderModifier = "* {0}";
            }

            if (effectiveModifiers.HasFlag(ScadNodeModifier.Color))
            {
                var colorModifier =
                    $"color([{effectiveColor.r}, {effectiveColor.g}, {effectiveColor.b}, {effectiveColor.a}]) {{0}}";
                if (renderModifier.Empty())
                {
                    renderModifier = colorModifier;
                }
                else
                {
                    renderModifier = string.Format(renderModifier, colorModifier);
                }
            }

            return renderModifier;
        }

        protected string RenderInput(IScadGraph context, int index)
        {
            return RenderPort(context, PortId.Input(index));
        }

        protected string RenderOutput(IScadGraph context, int index)
        {
            return RenderPort(context, PortId.Output(index));
        }

        public void BuildPortLiteral(PortId port)
        {
            var portDefinition = GetPortDefinition(port);

            if (!portDefinition.AllowLiteral)
            {
                return;
            }

            IScadLiteral literal;
            switch (portDefinition.PortType)
            {
                case PortType.Boolean:
                    literal = new BooleanLiteral(portDefinition.DefaultValueAsBoolean);
                    break;
                case PortType.Number:
                    literal = new NumberLiteral(portDefinition.DefaultValueAsDouble);
                    break;
                case PortType.String:
                    literal = new StringLiteral(portDefinition.DefaultValueAsString);
                    break;
                case PortType.Vector3:
                    literal = new Vector3Literal();
                    break;
                case PortType.Vector2:
                    literal = new Vector2Literal();
                    break;
                case PortType.Array:
                case PortType.Flow:
                case PortType.Any:
                    // nothing to do
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (port.IsInput)
            {
                _inputLiterals[port.Port] = literal;
            }
            else
            {
                _outputLiterals[port.Port] = literal;
            }
        }

        public void SetCustomAttribute(string key, string value)
        {
            _customAttributes[key] = value;
        }

        public bool TryGetCustomAttribute(string key, out string value)
        {
            return _customAttributes.TryGetValue(key, out value);
        }

        public void UnsetCustomAttribute(string key)
        {
            _customAttributes.Remove(key);
        }
    }
}