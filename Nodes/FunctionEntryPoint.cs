using System.Collections.Generic;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class FunctionEntryPoint : EntryPoint, IHaveMultipleExpressionOutputs, IReferToAFunction
    {
        private FunctionDescription _description;

        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;


        public InvokableDescription InvokableDescription => _description;

        static FunctionEntryPoint()
        {
            // a function entry point may not be disconnected from its return.
            ConnectionRules.AddDisconnectRule(
                it => it.From is FunctionEntryPoint && it.FromPort == 0,
                ConnectionRules.OperationRuleDecision.Veto
            );
        }

        public int GetParameterInputPort(int parameterIndex)
        {
            // entry point has no input ports that refer to parameters
            return -1;
        }

        public int GetParameterOutputPort(int parameterIndex)
        {
            // the n-th parameter is the n+1-th output port
            return parameterIndex + 1;
        }

        public IEnumerable<PortId> GetPortsReferringToReturnValue()
        {
            // entry point has no return value
            return Enumerable.Empty<PortId>();
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("function_description_id", _description.Id);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var functionDescriptionId = node.GetData("function_description_id");
            SetupPorts(referenceResolver.ResolveFunctionReference(functionDescriptionId));
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is FunctionDescription, "needs a function description");

            InputPorts.Clear();
            OutputPorts.Clear();

            _description = (FunctionDescription) description;
            OutputPorts
                .Flow();

            foreach (var parameter in description.Parameters)
            {
                OutputPorts
                    .OfType(parameter.TypeHint, parameter.Name, autoSetLiteralWhenPortIsDisconnected: false);
            }
        }

        public override string Render(IScadGraph context)
        {
            var arguments = _description.Parameters.Indices()
                .Select(it =>
                    _description.Parameters[it].Name + RenderOutput(context, it + 1).PrefixUnlessEmpty(" = "));

            return $"function {_description.Name}({string.Join(", ", arguments)}) = {RenderOutput(context, 0)};";
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            // return simply the parameter name.
            return _description.Parameters[port - 1].Name;
        }
    }
}