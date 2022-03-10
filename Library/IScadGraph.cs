using System.Collections.Generic;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public interface IScadGraph
    {
        string InvokableName { get; }

        void Blank(string name, ScadNode entryPoint);
        
        void LoadFrom(SavedGraph graph, ScadInvokableContext context);
        void SaveInto(SavedGraph graph);

        /// <summary>
        /// Returns the entrypoint node of the graph.
        /// </summary>
        ScadNode GetEntrypoint();

        IEnumerable<IScadConnection> GetAllConnections();

        void Discard();

    }
}