using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ListComprehension
{
    /// <summary>
    /// For comprehension node for building lists from other lists.
    /// </summary>
    [UsedImplicitly]
    public class ForComprehensionStart : ScadNode, IAmBoundToOtherNode, IAmAListComprehensionExpression, ICannotBeCreated
    {
        public override string NodeTitle => "For Comprehension";
        public override string NodeDescription => "Maps a list or range into a new list. Also known as a 'for' list comprehension.";

        public int NestLevel { get; private set; } = 1;
        
        public string OtherNodeId { get; set; }

        public ForComprehensionStart()
        {
            RebuildPorts();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                if (portId.Port < NestLevel)
                {
                    return "A list or range to be mapped.";
                }
            }

            if (portId.IsOutput)
            {
                if (portId.Port < NestLevel)
                {
                    return
                        "The current element of the input list. This can be used to create expressions that can be connected into the 'Result' input port.";
                }
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
                OutputPorts.OfType(PortType.Any, literalType:   LiteralType.Name);
            }
      
        }

        /// <summary>
        /// Adds a nested for loop level. The caller is responsible for fixing up port connections.
        /// </summary>
        public void IncreaseNestLevel()
        {
            NestLevel += 1;
            RebuildPorts();
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
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("nest_level", NestLevel);
            node.SetData("comprehensionEndId", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            NestLevel = node.GetDataInt("nest_level", 1);
            OtherNodeId = node.GetDataString("comprehensionEndId", "");
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