using System;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class RerouteNode : ScadNode, IMultiExpressionOutputNode
    {
        public override string NodeTitle => "Reroute";
        public override string NodeDescription => "Rerouting node which aids in making cleaner visual graphs.";


        public RerouteNode()
        {
            InputPorts
                .OfType(PortType.Reroute, allowLiteral: false);
            OutputPorts
                .OfType(PortType.Reroute, allowLiteral: false);
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("reroute_type", ((int) GetInputPortType(0)).ToString());
            base.SaveInto(node);
        }

        public override void LoadFrom(SavedNode node, IReferenceResolver referenceResolver)
        {
            var type = (PortType) int.Parse(node.GetData("reroute_type"));
            UpdatePortType(type);
            base.LoadFrom(node, referenceResolver);
        }

        public void UpdatePortType(PortType type)
        {
            InputPorts.Clear();
            OutputPorts.Clear();
            InputPorts
                .OfType(type, allowLiteral: false);
            OutputPorts
                .OfType(type, allowLiteral: false);
        }
        
        public override string Render(IScadGraph context)
        {
            if (GetOutputPortType(0) != PortType.Flow)
            {
                throw new InvalidOperationException("Cannot render non-flow type");
            }
            
            return RenderOutput(context, 0);
        }
        

        public string RenderExpressionOutput(IScadGraph context, int port)
        {

            var outputPortType = GetOutputPortType(0);
            if (outputPortType == PortType.Flow || outputPortType == PortType.Reroute)
            {
                throw new InvalidOperationException("Cannot render non-expression type.");
            }
            
            return RenderInput(context, 0);
        }

        public bool IsExpressionPort(int port)
        {
            if (port != 0)
            {
                return false;
            }
            
            var outputPortType = GetOutputPortType(0);
            return outputPortType != PortType.Flow && outputPortType != PortType.Reroute;
        }
    }
}