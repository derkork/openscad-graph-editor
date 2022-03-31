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

        public InvokableDescription InvokableDescription => _description;

        public override void SaveInto(SavedNode node)
        {
            base.SaveInto(node);
            node.SetData("function_description_id", _description.Id);
        }

        public int GetParameterInputPort(int parameterIndex)
        {
            // as function invocations have no flow ports, the n-th parameter port is the n-th input port.
            return parameterIndex;
        }
        

        public int GetParameterOutputPort(int parameterIndex)
        {
            // function invocations have no output ports corresponding to the parameters.
            return -1;
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var functionDescriptionId = node.GetData("function_description_id");
            SetupPorts(referenceResolver.ResolveFunctionReference(functionDescriptionId));
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is FunctionDescription, "Needs a function description");
            InputPorts.Clear();
            OutputPorts.Clear();
            
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