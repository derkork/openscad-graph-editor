using System.Collections.Generic;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public interface IScadGraph : ICanBeRendered
    {
        InvokableDescription Description { get; }
        
        void LoadFrom(SavedGraph graph, IReferenceResolver resolver);
        
        void SaveInto(SavedGraph graph);

        IEnumerable<ScadConnection> GetAllConnections();

        void Discard();

    }
}