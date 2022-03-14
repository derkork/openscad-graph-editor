using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class FunctionReturn : ScadNode, ICannotBeDeleted, ICannotBeCreated
    {
        private FunctionDescription _description;

        public override string NodeTitle => "Return value";
        public override string NodeDescription => "Returns the given value.";


        public override void SaveInto(SavedNode node)
        {
            node.SetData("function_description_id", _description.Id);
            base.SaveInto(node);
        }

        public override void LoadFrom(SavedNode node, IReferenceResolver referenceResolver)
        {
            _description = referenceResolver.ResolveFunctionReference(node.GetData("function_description_id"));
            Setup(_description);
            base.LoadFrom(node, referenceResolver);
        }

        public void Setup(FunctionDescription description)
        {
            _description = description;
            InputPorts
                .Flow()
                .OfType(_description.ReturnTypeHint, "Result");
        }
        
        public override string Render(IScadGraph context)
        {
            return RenderInput(context, 1);
        }
    }
}