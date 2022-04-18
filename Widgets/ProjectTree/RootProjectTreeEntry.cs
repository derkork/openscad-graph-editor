using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public class RootProjectTreeEntry : ProjectTreeEntry<ScadProject>
    {
        public override string Title => "Project";
        public override bool CanBeDragged => false;
        public override bool CanBeActivated => false;
        public override Texture Icon => null;
        public override string Id => "e954fea078764fb9b7c95b8db62a0f56"; // we will only have one of these, so it can have a static id
        public override ScadProject Description { get; }

        public override List<ProjectTreeEntry> Children { get; }

        public RootProjectTreeEntry(ScadProject project)
        {
            Description = project;
            
            var list = new List<ProjectTreeEntry> {new ScadMainModuleTreeEntry(project.MainModule.Description)};
            list.AddRange(project.Functions.OrderBy(it => it.Description.Name).Select(f => new ScadInvokableTreeEntry(f.Description)));
            list.AddRange(project.Modules.OrderBy(it => it.Description.Name).Select(m => new ScadInvokableTreeEntry(m.Description)));
            list.AddRange(project.Variables.OrderBy(it => it.Name).Select(v => new ScadVariableTreeEntry(v)));
            list.AddRange(project.ExternalReferences.Select(e => new ExternalReferenceTreeEntry(e)));

            Children = list;

        }

    }
}