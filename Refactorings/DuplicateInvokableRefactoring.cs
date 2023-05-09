using System;
using Godot.Collections;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class DuplicateInvokableRefactoring : Refactoring
    {
        private readonly InvokableDescription _toDuplicate;
        
        public DuplicateInvokableRefactoring(InvokableDescription toDuplicate)
        {
            _toDuplicate = toDuplicate;
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            GdAssert.That(context.Project.IsDefinedInThisProject(_toDuplicate), 
                "Tried to duplicate a function that is not defined in this project");

            var graph = context.Project.FindDefiningGraph(_toDuplicate);
            context.Project.AddInvokable(Duplicate(graph, context));
        }

        private ScadGraph Duplicate(ScadGraph graph, RefactoringContext context)
        {
            var savedGraph = new SavedGraph();
            // serialize the graph into a saved graph
            graph.SaveInto(savedGraph);
            
            // modify the ID of the saved invokable description
            savedGraph.Description.Id = Guid.NewGuid().ToString();
            // give the duplicate a unique new name
            savedGraph.Description.Name = context.Project.SafeName(savedGraph.Description.Name);            
            
            // now we need to patch up all node IDs in the saved graph and replace them with new ones
            // this also means that we need to patch the connections in the saved graph
            
            // dictionary for saving the old ID and the new ID
            var idMapping = new Dictionary<string, string>();
            
            foreach (var node in savedGraph.Nodes)
            {
                var oldId = node.Id;
                node.Id = Guid.NewGuid().ToString();
                idMapping[oldId] = node.Id;
                foreach (var connection in savedGraph.Connections)
                {
                    if (connection.FromId == oldId)
                    {
                        connection.FromId = node.Id;
                    }
                    if (connection.ToId == oldId)
                    {
                        connection.ToId = node.Id;
                    }
                }
            }
            
            // now create the duplicate graph and load the saved graph into it
            var duplicate = new ScadGraph();
            // load the graph again with the patched description
            duplicate.LoadFrom(savedGraph, savedGraph.Description.FromSavedState(), context.Project);
            
            // now the graph may contain nodes which implement IAmBoundToOtherNode. These
            // serialize their bound node ID into the saved graph. We need to patch these IDs
            // as well.
            foreach (var node in duplicate.GetAllNodes())
            {
                if (node is IAmBoundToOtherNode boundNode)
                {
                    // replace with the new ID
                    boundNode.OtherNodeId = idMapping[boundNode.OtherNodeId];
                }
            }
            
            // the graph may also contain nodes which implement IReferToAnInvokable which may refer
            // to the invokable we are duplicating (eg. entry points, return points,
            // recursive invocations). We need to patch these references as well.
            foreach (var node in duplicate.GetAllNodes())
            {
                if (node is IReferToAnInvokable referNode && referNode.InvokableDescription == _toDuplicate)
                {
                    // replace with the clone
                    referNode.SetupPorts(duplicate.Description);
                }
            }
            
            return duplicate;
        }
    }
}