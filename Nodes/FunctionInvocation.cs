using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A function invocation.
    /// </summary>
    public class FunctionInvocation : ScadExpressionNode
    {
        private FunctionDescription _description;
        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;


        public override void SaveInto(SavedNode node)
        {
            base.SaveInto(node);
            node.SetData("function_description_id", _description.Id);
        }

        public override void LoadFrom(SavedNode node)
        {
            var functionDescriptionId = node.GetData("function_description_id");
            Setup(InvokableLibrary.ForFunctionDescriptionId(functionDescriptionId));
            base.LoadFrom(node);
        }

        public void Setup(FunctionDescription description)
        {
            _description = description;

            foreach (var parameter in description.Parameters)
            {
                var type = parameter.TypeHint;
                InputPorts
                    .OfType(type, parameter.LabelOrFallback, type != PortType.Any && type != PortType.Flow,
                        parameter.IsAutoCoerced);
            }

            OutputPorts
                    .OfType(description.ReturnTypeHint, allowLiteral: false);
        }

        public override string Render(ScadInvokableContext scadInvokableContext)
        {
            
            var parameters = string.Join(", ",
                InputPorts
                    .Indices()
                    .Select(it =>
                    {
                        var parameterName = _description.Parameters[it].Name;
                        return parameterName.Length > 0 ? $"{parameterName} = {RenderInput(scadInvokableContext, it)}" : RenderInput(scadInvokableContext, it);
                    })
            );
            return $"{_description.Name}({parameters})";
        }
    }
}