using System.Collections.Generic;
using Godot;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public abstract class ProjectTreeEntry
    {
        public abstract string Title { get; }
        public abstract bool CanBeDragged { get; }
        public abstract bool CanBeActivated { get; }
        
        public virtual bool CanBeCollapsed => true;

        public abstract Texture Icon { get; }
        
        public abstract string Id { get; }

        public virtual List<ProjectTreeEntry> Children => new List<ProjectTreeEntry>();
    }
    
    public abstract class ProjectTreeEntry<T> : ProjectTreeEntry
    {
        public abstract T Description { get; }
    }
}