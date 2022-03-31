using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class SetVariable : ScadNode, IReferToAVariable
    {
        private VariableDescription _description;
        public override string NodeTitle => $"Set {_description?.Name ?? "Variable"}";
        public override string NodeDescription => "Sets a variable.";

        
        public SetVariable()
        {
            InputPorts
                .Flow()
                .Any("Value");

            OutputPorts
                .Flow();
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
            var value = RenderInput(context, 1);
            var next = RenderOutput(context, 0);
            return $"{_description.Name} = {value};\n{next}";
        }

        public void SetupPorts(VariableDescription description)
        {
            _description = description;
        }
    }
}