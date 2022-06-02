using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ForLoop
{
    [UsedImplicitly]
    public class ForLoopStart : ScadNode, ICannotBeCreated, IAmBoundToOtherNode
    {
        public override string NodeTitle => "Start Loop";
        public override string NodeDescription => "Begins a loop. The loop will be executed for each element in the given vector.";

        public int NestLevel { get; private set; } = 1;
        
        public bool IntersectMode { get; set; }

        public string OtherNodeId { get; set; }

        public ForLoopStart()
        {
            RebuildPorts();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "Vector to iterate over";
            }

            if (portId.IsOutput)
            {
                return "The current element of the vector from the left. You may give it a custom name. If you don't give it a name, one will be generated.";
            }

            return "";
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();

            for (var i = 0; i < NestLevel; i++)
            {
                InputPorts.Array();
                OutputPorts.OfType(PortType.Any, literalType: LiteralType.Name);
            }
        }

        /// <summary>
        /// Adds a nested for loop level. The caller is responsible for fixing up port connections.
        /// </summary>
        public void IncreaseNestLevel()
        {
            NestLevel += 1;
            RebuildPorts();
            // add a port literal for the new variable
            BuildPortLiteral(PortId.Output(NestLevel-1));
        }

        /// <summary>
        /// Removes a nested for loop level. The caller is responsible for fixing up port connections.
        /// </summary>
        public void DecreaseNestLevel()
        {
            GdAssert.That(NestLevel > 1, "Cannot decrease nest level any further.");
            DropPortLiteral(PortId.Output(NestLevel-1));
            NestLevel -= 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("nest_level", NestLevel);
            node.SetData("intersect_mode", IntersectMode);
            node.SetData("loop_end_id", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            NestLevel = node.GetDataInt("nest_level", 1);
            IntersectMode = node.GetDataBool("intersect_mode");
            OtherNodeId = node.GetDataString("loop_end_id");
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex < 0 || portIndex >= NestLevel)
            {
                return "";
            }

            return RenderOutput(context, portIndex).OrDefault(Id.UniqueStableVariableName(portIndex));
        }
    }
}