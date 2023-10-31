using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes.StringEnum
{
    /// <summary>
    /// A node that checks if a string is one of the given enum values.
    /// </summary>
    [UsedImplicitly]
    public class StringEnum : ScadNode, IAmAnExpression, IHaveVariableOutputSize
    {
        public override string NodeTitle => "String Enum";
        public override string NodeDescription => "Checks if a string is one of the given enum values.";
        public override string NodeQuickLookup => "Senum";
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add enum value";
        public string RemoveRefactoringTitle => "Remove enum value";
        public int CurrentOutputSize { get; private set; } = 1;
        

        public StringEnum()
        {
            RebuildPorts();
        }
        
        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "The string to check";
            }

            return "True if the string is one of the given enum values, false otherwise.";
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();
            
            InputPorts.String(allowLiteral: false);

            for (var i = 0; i < CurrentOutputSize; i++)
            {
                OutputPorts.PortType(PortType.Boolean, literalType: LiteralType.String);
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var stringValue = RenderOutput(context, portIndex);
            var source = RenderInput(context, 0);
            
            return $"({source} == {stringValue})";
        }
        
        public override void SaveInto(SavedNode node)
        {
            node.SetData("number_of_outputs",CurrentOutputSize);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            CurrentOutputSize = node.GetDataInt("number_of_outputs", 1);
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }
        
        public void AddVariableOutputPort()
        {
            CurrentOutputSize += 1;
            RebuildPorts();
            // add a port literal for the new variable
            BuildPortLiteral(PortId.Output(CurrentOutputSize-1));
        }

        public void RemoveVariableOutputPort()
        {
            GdAssert.That(CurrentOutputSize > 1, "Cannot decrease remove last output further.");
            DropPortLiteral(PortId.Output(CurrentOutputSize-1));
            CurrentOutputSize -= 1;
            RebuildPorts();
        }


    }
}