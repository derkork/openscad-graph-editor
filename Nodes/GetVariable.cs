using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class GetVariable : ScadExpressionNode, IReferToAVariable
    {
        private VariableDescription _description;
        public override string NodeTitle => $"Get {_description?.Name ?? "Variable"}";
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
            _description = description;
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("variable_description_id", _description.Id);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            _description = referenceResolver.ResolveVariableReference(node.GetData("variable_description_id")); 
        }

        public override string Render(IScadGraph context)
        {
            return $"{_description.Name}";
        }
    }
}