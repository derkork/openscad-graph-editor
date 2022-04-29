using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class SetVariable : ScadNode, IReferToAVariable
    {
        public VariableDescription VariableDescription { get; private set; }
        public override string NodeTitle => $"Set {VariableDescription?.Name ?? "Variable"}";
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
            var value = RenderInput(context, 1);
            var next = RenderOutput(context, 0);
            return $"{VariableDescription.Name} = {value};\n{next}";
        }

        public void SetupPorts(VariableDescription description)
        {
            VariableDescription = description;
        }
    }
}