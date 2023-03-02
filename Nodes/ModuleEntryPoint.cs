using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class ModuleEntryPoint : EntryPoint, IReferToAnInvokable
    {
        private ModuleDescription _description;

        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeQuickLookup => "";
        public override string NodeDescription => _description.Description;

        public InvokableDescription InvokableDescription => _description;

        public override void SaveInto(SavedNode node)
        {
            node.SetData("module_description_id", _description.Id);
            base.SaveInto(node);
        }

        
        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsOutput)
            {
                return _description.Parameters[portId.Port].Description;
            }

            return "";
        }
        
        public int GetParameterInputPort(int parameterIndex)
        {
            // module entry points have no input ports.
            return -1;
        }

        public int GetParameterOutputPort(int parameterIndex)
        {
            // the n-th parameter is at the n-th output port.
            return parameterIndex;
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var functionDescriptionId = node.GetDataString("module_description_id");
            SetupPorts(referenceResolver.ResolveModuleReference(functionDescriptionId));
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is ModuleDescription, "needs a module description");
            InputPorts.Clear();
            OutputPorts.Clear();

            _description = (ModuleDescription) description;

            foreach (var parameter in description.Parameters)
            {
                OutputPorts
                    .PortType(parameter.TypeHint, parameter.Name, autoSetLiteralWhenPortIsDisconnected: false, literalType: parameter.TypeHint.GetMatchingLiteralType());
            }
        }

        public override string RenderEntryPoint(string content)
        {
            var arguments = _description.Parameters.Indices()
                .Select(it =>
                    _description.Parameters[it].Name + RenderLiteral(PortId.Output(it)).PrefixUnlessEmpty(" = "));

            var comment = RenderDocumentationComment(_description);
            return $"{comment}\nmodule {_description.Name}({string.Join(", ", arguments)}){content.AsBlock()}";
        }
        
        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex < 0 || portIndex >= OutputPorts.Count)
            {
                return "";
            }

            // return simply the parameter name.
            return _description.Parameters[portIndex].Name;
               
        }

    }
}