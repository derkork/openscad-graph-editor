using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class ModuleInvocation : ScadNode, IReferToAnInvokable, ICanHaveModifier
    {
        private ModuleDescription _description;
        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;

        public InvokableDescription InvokableDescription => _description;

        public override void SaveInto(SavedNode node)
        {
            base.SaveInto(node);
            node.SetData("module_description_id", _description.Id);
        }

        public int GetParameterInputPort(int parameterIndex)
        {
            // the n-th parameter corresponds to the n+1-th input port
            return parameterIndex + 1;
        }

        public int GetParameterOutputPort(int parameterIndex)
        {
            // no output ports correspond to parameters
            return -1;
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var moduleDescriptionId = node.GetData("module_description_id");
            SetupPorts(referenceResolver.ResolveModuleReference(moduleDescriptionId));
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is ModuleDescription, "needs a module description");
            _description = (ModuleDescription) description;
            InputPorts.Clear();
            OutputPorts.Clear();

            InputPorts
                .Flow();

            foreach (var parameter in description.Parameters)
            {
                var type = parameter.TypeHint;
                InputPorts
                    .OfType(type, parameter.LabelOrFallback, true);
            }

            if (_description.SupportsChildren)
            {
                OutputPorts
                    .Flow("Children")
                    .Flow("After");
            }
            else
            {
                OutputPorts
                    .Flow();
            }
        }


        public override string Render(IScadGraph context)
        {
            var parameters = _description.Parameters.Count.Range()
                .Select(it =>
                {
                    var parameterDescription = _description.Parameters[it];
                    var value = RenderInput(context, it + 1);
                    return value.Empty() && parameterDescription.IsOptional
                        ? ""
                        : $"{parameterDescription.Name} = {value.OrUndef()}";
                })
                .Where(it => !it.Empty())
                .JoinToString(", ");
       
            var result = $"{_description.Name}({parameters})";
            var childNodes = _description.SupportsChildren ? RenderOutput(context, 0) : "";
            var nextNodes = RenderOutput(context, _description.SupportsChildren ? 1 : 0);
            if (childNodes.Length > 0)
            {
                return result + childNodes.AsBlock() + nextNodes;
            }

            return result + ";\n" + nextNodes;
        }
    }
}