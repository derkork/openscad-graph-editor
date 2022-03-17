using GodotExt;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class FunctionReturn : ScadNode, ICannotBeDeleted, IReferToAnInvokable
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

        public void Setup(InvokableDescription description)
        {
            GdAssert.That(description is FunctionDescription, "need a function description");
            _description = (FunctionDescription) description;
            
            InputPorts
                .Flow()
                .OfType(_description.ReturnTypeHint, "Result");
        }
        
        public override string Render(IScadGraph context)
        {
            var input = RenderInput(context, 1);
            return input.Length == 0 ? "undef" : input;
        }
    }
}