using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public static class RefactoringExt
    {
        /// <summary>
        /// Finds all nodes in the project that refer to the given <see cref="InvokableDescription"/>
        /// </summary>
        public static IEnumerable<ReferencingNode<IReferToAnInvokable, IScadGraph>> FindAllReferencingNodes(
            this ScadProject project, InvokableDescription description)
        {
            var graphs = project.AllDeclaredInvokables;
            foreach (var graph in graphs)
            {
                foreach (var node in graph.GetAllNodes().OfType<IReferToAnInvokable>()
                             .Where(it => it.InvokableDescription == description))
                {
                    yield return new ReferencingNode<IReferToAnInvokable, IScadGraph>(graph, (ScadNode) node, node);
                }
            }
        }

        /// <summary>
        /// Finds all nodes in the project that refer to the given <see cref="VariableDescription"/>
        /// </summary>
        public static IEnumerable<ReferencingNode<IReferToAVariable, IScadGraph>> FindAllReferencingNodes(
            this ScadProject project,
            VariableDescription description)
        {
            var graphs = project.AllDeclaredInvokables;
            foreach (var graph in graphs)
            {
                foreach (var node in graph.GetAllNodes().OfType<IReferToAVariable>()
                             .Where(it => it.VariableDescription == description))
                {
                    yield return new ReferencingNode<IReferToAVariable, IScadGraph>(graph, (ScadNode) node, node);
                }
            }
        }
        

        public readonly struct ReferencingNode<T, TGraphType> where TGraphType : IScadGraph
        {
            public TGraphType Graph { get; }
            public ScadNode Node { get; }
            public T NodeAsReference { get; }
            
            public ReferencingNode(TGraphType graph, ScadNode node, T nodeAsReference)
            {
                Node = node;
                NodeAsReference = nodeAsReference;
                Graph = graph;
            }
        }
    }
}