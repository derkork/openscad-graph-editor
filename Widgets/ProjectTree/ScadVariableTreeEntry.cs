using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public class ScadVariableTreeEntry : ProjectTreeEntry<VariableDescription>
    {
        public override Texture Icon { get; }

        public override string Id => Description.Id;

        public override string Title => Description.Name;
        public override bool CanBeDragged => true;
        public override bool CanBeActivated => true;
        public override VariableDescription Description { get; }

        public ScadVariableTreeEntry(VariableDescription description)
        {
            Description = description;
            Icon = Resources.VariableIcon;
        }

    }
}