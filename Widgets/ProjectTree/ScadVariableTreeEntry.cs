using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public class ScadVariableTreeEntry : ProjectTreeEntry<VariableDescription>
    {
        public override Texture Icon { get; }

        public override string Id => Description.Id;

        public override string Title => $"{Description.Name} <{Description.TypeHint.HumanReadableName()}>";
        public override VariableDescription Description { get; }

        public override bool TryGetDragData(out object data)
        {
            data = Description;
            return true;
        }

        public ScadVariableTreeEntry(VariableDescription description)
        {
            Description = description;
            Icon = Resources.VariableIcon;
        }

    }
}