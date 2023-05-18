using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public class ScadInvokableTreeEntry : ProjectTreeEntry<InvokableDescription>
    {
        public override Texture Icon { get; }
        public override string Id => Description.Id;

        public override string Title => Description.Name;
        public override InvokableDescription Description { get; }


        public override bool TryGetDragData(out object data)
        {
            data = Description;
            return true;
        }

        public ScadInvokableTreeEntry(InvokableDescription description)
        {
            Description = description;
            
            if (description is FunctionDescription)
            {
                Icon = Resources.FunctionIcon;
            }
            if (description is ModuleDescription)
            {
                Icon = Resources.ModuleIcon;
            }
        }
    }
}