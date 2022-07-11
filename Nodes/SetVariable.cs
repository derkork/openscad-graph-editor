using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class SetVariable : ScadNode, IReferToAVariable
    {
        public VariableDescription VariableDescription { get; private set; }
        public override string NodeTitle => $"Set {VariableDescription?.Name ?? "Variable"}";
        public override string NodeDescription => "Sets a variable. Note, that in OpenSCAD variables are global and the last assignment to a variable will be used. This can be counter-intuitive. If you need a temporary variable use a 'let' block.";

        // no point in having a shortcut as this is repeated for each variable
        public override string NodeQuickLookup => "";

        public SetVariable()
        {
            InputPorts
                .Any("Value");

            OutputPorts
                .Geometry();
        }


        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The value to which the variable should be set";
                case 0 when portId.IsOutput:
                    return "Output 'geometry'. Variable assignments will not actually output geometry, but this can be used to link the assignment to a specific scope.";
                default:
                    return "";
            }
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("variable_description_id", VariableDescription.Id);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            VariableDescription = referenceResolver.ResolveVariableReference(node.GetDataString("variable_description_id")); 
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }
            
            var value = RenderInput(context, 1).OrUndef();
            var before = RenderInput(context, 0);
            return $"{before}\n{VariableDescription.Name} = {value};";
        }

        public void SetupPorts(VariableDescription description)
        {
            VariableDescription = description;
        }
    }
}