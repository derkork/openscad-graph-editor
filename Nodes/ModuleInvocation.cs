using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class ModuleInvocation : ScadNode, IReferToAnInvokable
    {
        private ModuleDescription _description;
        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;


        public override void SaveInto(SavedNode node)
        {
            base.SaveInto(node);
            node.SetData("module_description_id", _description.Id);
        }

        public override void LoadFrom(SavedNode node, IReferenceResolver referenceResolver)
        {
            var moduleDescriptionId = node.GetData("module_description_id");
            Setup(referenceResolver.ResolveModuleReference(moduleDescriptionId));
            base.LoadFrom(node, referenceResolver);
        }

        public void Setup(InvokableDescription description)
        {
            GdAssert.That(description is ModuleDescription, "needs a module description");
            _description = (ModuleDescription) description;

            InputPorts
                .Flow();

            foreach (var parameter in description.Parameters)
            {
                var type = parameter.TypeHint;
                InputPorts
                    .OfType(type, parameter.LabelOrFallback, true, parameter.IsAutoCoerced);
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
            var parameters = string.Join(", ",
                InputPorts
                    .Indices()
                    .Skip(1)
                    .Select(it => $"{_description.Parameters[it - 1].Name} = {RenderInput(context, it)}")
            );
            var result = $"{_description.Name}({parameters})";
            var childNodes = _description.SupportsChildren ? RenderOutput(context, 0) : "";
            var nextNodes = RenderOutput(context, _description.SupportsChildren ? 1 : 0);
            if (childNodes.Length > 0)
            {
                return result + childNodes.AsBlock() + ";" + nextNodes;
            }

            return result + ";\n" + nextNodes;
        }
    }
}