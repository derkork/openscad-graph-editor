using System.Collections.Generic;
using GodotExt;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class FunctionReturn : ScadNode, ICannotBeDeleted, IReferToAFunction
    {
        private FunctionDescription _description;

        public override string NodeTitle => "Return value";
        public override string NodeDescription => "Returns the given value.";

        public InvokableDescription InvokableDescription => _description;

        public override void SaveInto(SavedNode node)
        {
            node.SetData("function_description_id", _description.Id);
            base.SaveInto(node);
        }

        public int GetParameterInputPort(int parameterIndex)
        {
            // no ports refer to parameters.
            return -1;
        }

        public int GetParameterOutputPort(int parameterIndex)
        {
            // no ports refer to parameters.
            return -1;
        }


        public IEnumerable<PortId> GetPortsReferringToReturnValue()
        {
            return new[] {PortId.Input(1)};
        }


        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            _description = referenceResolver.ResolveFunctionReference(node.GetData("function_description_id"));
            SetupPorts(_description);
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is FunctionDescription, "need a function description");
            InputPorts.Clear();
            OutputPorts.Clear();

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