using System.Collections.Generic;

namespace OpenScadGraphEditor.Library
{
    public interface IScadGraph : ICanBeRendered
    {
        InvokableDescription Description { get; }
        
        void LoadFrom(SavedGraph graph, IReferenceResolver resolver);
        
        void SaveInto(SavedGraph graph);

        IEnumerable<IScadConnection> GetAllConnections();

        void Discard();

    }
}