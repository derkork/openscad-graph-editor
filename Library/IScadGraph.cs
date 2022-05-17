using System.Collections.Generic;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public interface IScadGraph : ICanBeRendered
    {
        InvokableDescription Description { get; }

        void LoadFrom(SavedGraph graph, InvokableDescription description, IReferenceResolver resolver);
        
        void SaveInto(SavedGraph graph);

        IEnumerable<ScadConnection> GetAllConnections();

        IEnumerable<ScadNode> GetAllNodes();

        void Discard();

        // TODO: make sure this is only ever called through a refactoring
        void RemoveConnection(ScadConnection it);
        // TODO: this can use nodes now, no ids.
        void AddConnection(string fromId, int fromPort, string toId, int toPort);
        void AddNode(ScadNode node);
        void RemoveNode(ScadNode node);
    }
}