using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class GetVariable : ScadNode, IReferToAVariable, IAmAnExpression
    {
        public VariableDescription VariableDescription { get; private set; }
        public override string NodeTitle => $"Get {VariableDescription?.Name ?? "Variable"}";
        public override string NodeDescription => "Gets a variable's value.";

        
        public GetVariable()
        {
            // no input ports

            // since variables have no defined type, we need to use "Any" as output type.
            OutputPorts
                .Any();
        }


        public void SetupPorts(VariableDescription description)
        {
            VariableDescription = description;
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("variable_description_id", VariableDescription.Id);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            VariableDescription = referenceResolver.ResolveVariableReference(node.GetData("variable_description_id")); 
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(IScadGraph context)
        {
            return $"{VariableDescription.Name}";
        }
    }
}