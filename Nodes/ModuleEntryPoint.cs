using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class ModuleEntryPoint : EntryPoint, IHaveMultipleExpressionOutputs, IReferToAnInvokable
    {
        private ModuleDescription _description;

        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;

        public InvokableDescription InvokableDescription => _description;

        public override void SaveInto(SavedNode node)
        {
            node.SetData("module_description_id", _description.Id);
            base.SaveInto(node);
        }

        public int GetParameterInputPort(int parameterIndex)
        {
            // module entry points have no input ports.
            return -1;
        }

        public int GetParameterOutputPort(int parameterIndex)
        {
            // the n-th parameter is at the n+1-th output port.
            return parameterIndex + 1;
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var functionDescriptionId = node.GetData("module_description_id");
            SetupPorts(referenceResolver.ResolveModuleReference(functionDescriptionId));
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is ModuleDescription, "needs a module description");
            InputPorts.Clear();
            OutputPorts.Clear();

            _description = (ModuleDescription) description;
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
            var content = RenderOutput(context, 0);

            return $"module {_description.Name}({string.Join(", ", arguments)}){content.AsBlock()}";
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            // return simply the parameter name.
            return _description.Parameters[port - 1].Name;
        }
    }
}