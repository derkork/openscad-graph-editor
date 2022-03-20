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
        
        
        public IScadLiteral GetInputLiteral(int index)
        {
            return _inputLiterals.TryGetValue(index, out var result) ? result : default;
        }
        
        public IScadLiteral GetOutputLiteral(int index)
        {
            return _outputLiterals.TryGetValue(index, out var result) ? result : default;
        }

        public PortDefinition GetInputPortDefinition(int index)
        {
            return InputPorts[index];
        }

        public PortDefinition GetOutputPortDefinition(int index)
        {
            return OutputPorts[index];
        }
        
        public virtual void SaveInto(SavedNode node)
        {
            node.Id = Id;
            node.Type = GetType().FullName;
            node.Position = Offset;
            InputPorts
                .Indices()
                .Where(i => _inputLiterals.ContainsKey(i))
                .ForAll(i => node.SetData($"input_literal_value.{i}", _inputLiterals[i].SerializedValue));
            OutputPorts
                .Indices()
                .Where(i => _outputLiterals.ContainsKey(i))
                .ForAll(i => node.SetData($"output_literal_value.{i}", _outputLiterals[i].SerializedValue));
        }


        public void PreparePorts()
        {
            var maxPorts = Mathf.Max(InputPorts.Count, OutputPorts.Count);
            var idx = 0;
            while (idx < maxPorts)
            {
                if (InputPorts.Count > idx)
                {
                    BuildPort(idx, InputPorts[idx], true);
                }

                if (OutputPorts.Count > idx)
                {
                    BuildPort(idx, OutputPorts[idx], false);
                }

                idx++;
            }
        }

        public virtual void LoadFrom(SavedNode node, IReferenceResolver referenceResolver)
        {
            PreparePorts();
            Id = node.Id;
            Offset = node.Position;
            InputPorts
                .Indices()
                .Where(i => _inputLiterals.ContainsKey(i))
                .ForAll(i => _inputLiterals[i].SerializedValue = node.GetData($"input_literal_value.{i}"));
            OutputPorts
                .Indices()
                .Where(i => _outputLiterals.ContainsKey(i))
                .ForAll(i => _outputLiterals[i].SerializedValue = node.GetData($"output_literal_value.{i}"));
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

        public abstract string Render(IScadGraph context);

        protected string RenderInput(IScadGraph context, int index)
        {
            // if we have a node connected, render the node
            if (context.TryGetIncomingNode(this, index, out var node, out var fromPort))
            {
                if (node is IMultiExpressionOutputNode multiNode)
                {
                    return multiNode.RenderExpressionOutput(context, fromPort);
                }
                
                return node.Render(context);
            }

            // try rendering the literal
            if (_inputLiterals.TryGetValue(index, out var literal))
            {
                return literal.LiteralValue;
            }

            // otherwise return nothing
            return "";
        }

        protected string RenderOutput(IScadGraph context, int index)
        {
            // if we have a flow node connected render this.
            if (GetOutputPortType(index) == PortType.Flow)
            {
                if (context.TryGetOutgoingNode(this, index, out var node, out _))
                {
                    return node.Render(context);
                }
            }

            // try rendering the literal
            if (_outputLiterals.TryGetValue(index, out var literal))
            {
                return literal.LiteralValue;
            }

            // otherwise return nothing
            return "";
        }


        private void BuildPort(int idx, PortDefinition portDefinition, bool isLeft)
        {
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
                case PortType.Array:
                case PortType.Flow:
                case PortType.Any:
                    throw new ArgumentException(
                        $"Port type {portDefinition.PortType} cannot have a literal input.");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isLeft)
            {
                _inputLiterals[idx] = literal;
            }
            else
            {
                _outputLiterals[idx] = literal;
            }
        }
    }
}