using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A function invocation.
    /// </summary>
    public class FunctionInvocation : ScadExpressionNode, IReferToAnInvokable
    {
        private FunctionDescription _description;
        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;


        public override void SaveInto(SavedNode node)
        {
            base.SaveInto(node);
            node.SetData("function_description_id", _description.Id);
        }

        public override void LoadFrom(SavedNode node, IReferenceResolver referenceResolver)
        {
            var functionDescriptionId = node.GetData("function_description_id");
            Setup(referenceResolver.ResolveFunctionReference(functionDescriptionId));
            base.LoadFrom(node, referenceResolver);
        }

        public void Setup(InvokableDescription description)
        {
            GdAssert.That(description is FunctionDescription, "Needs a function description");
            
            _description = (FunctionDescription) description;

            foreach (var parameter in _description.Parameters)
            {
                var type = parameter.TypeHint;
                InputPorts
                    .OfType(type, parameter.LabelOrFallback, true, parameter.IsAutoCoerced);
            }

            OutputPorts
                    .OfType( _description.ReturnTypeHint, allowLiteral: false);
        }

        public override string Render(IScadGraph context)
        {
            
            var parameters = string.Join(", ",
                InputPorts
                    .Indices()
                    .Select(it =>
                    {
                        var parameterName = _description.Parameters[it].Name;
                        return parameterName.Length > 0 ? $"{parameterName} = {RenderInput(context, it)}" : RenderInput(context, it);
                    })
            );
            return $"{_description.Name}({parameters})";
        }
    }
}