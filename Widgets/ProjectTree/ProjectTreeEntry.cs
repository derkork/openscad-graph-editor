using System.Collections.Generic;
using Godot;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public abstract class ProjectTreeEntry
    {
        public abstract string Title { get; }
        
        public virtual bool CanBeCollapsed => true;

        public abstract Texture Icon { get; }
        
        public abstract string Id { get; }

        public virtual List<ProjectTreeEntry> Children => new List<ProjectTreeEntry>();

        public virtual bool TryGetDragData(out object data)
        {
            data = default;
            return false;
        }
    }
    
    public abstract class ProjectTreeEntry<T> : ProjectTreeEntry
    {
        public abstract T Description { get; }
    }
}