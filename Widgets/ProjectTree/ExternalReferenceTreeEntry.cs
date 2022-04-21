using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public class ExternalReferenceTreeEntry : ProjectTreeEntry<ExternalReference>
    {
        public override string Title => Description.SourceFile;
        public override ExternalReference Description { get; }
        public override bool CanBeDragged => false;
        public override bool CanBeActivated => true;
        public override Texture Icon => Description.IsTransitive ? Resources.TransitiveImportIcon : Resources.ImportIcon;
        public override string Id => Description.Id;

        public override List<ProjectTreeEntry> Children { get; }

        public ExternalReferenceTreeEntry(ExternalReference externalReference)
        {
            Description = externalReference;
            
            var list = new List<ProjectTreeEntry>();
            list.AddRange(externalReference.Functions.OrderBy(it => it.Name).Select(f => new ScadInvokableTreeEntry(f)));
            list.AddRange(externalReference.Modules.OrderBy(it => it.Name).Select(m => new ScadInvokableTreeEntry(m)));
            list.AddRange(externalReference.Variables.OrderBy(it => it.Name).Select(v => new ScadVariableTreeEntry(v)));

            Children = list;

        }

    }
}