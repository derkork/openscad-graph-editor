using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public class ScadMainModuleTreeEntry : ScadInvokableTreeEntry
    {
        
        public override bool CanBeDragged => false;

        public override Texture Icon => Resources.ModuleIcon;

        public ScadMainModuleTreeEntry(InvokableDescription description) : base(description)
        {
        }
    }
}