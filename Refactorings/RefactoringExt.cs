using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
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
            var graphs = project.Functions.Concat(project.Modules).Append(project.MainModule);
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
            var graphs = project.Functions.Concat(project.Modules).Append(project.MainModule);
            foreach (var graph in graphs)
            {
                foreach (var node in graph.GetAllNodes().OfType<IReferToAVariable>()
                             .Where(it => it.VariableDescription == description))
                {
                    yield return new ReferencingNode<IReferToAVariable, IScadGraph>(graph, (ScadNode) node, node);
                }
            }
        }

        /// <summary>
        /// Finds all nodes in the project that refer to the given <see cref="ExternalReference"/>
        /// </summary>
        public static IEnumerable<ReferencingNode<IReferToAnExternalReference, IScadGraph>> FindAllReferencingNodes(
            this ScadProject project,
            ExternalReference externalReference)
        {
            var graphs = project.Functions.Concat(project.Modules).Append(project.MainModule);
            foreach (var graph in graphs)
            {
                foreach (var node in graph.GetAllNodes().OfType<IReferToAnExternalReference>()
                             .Where(it => it.ExternalReference == externalReference))
                {
                    yield return new ReferencingNode<IReferToAnExternalReference, IScadGraph>(graph, (ScadNode) node,
                        node);
                }
            }
        }

        /**
         * Makes the graph of the given referencing node refactorable and returns an updated referencing node.
         */
        public static ReferencingNode<IReferToAnExternalReference, LightWeightGraph> MakeRefactorable<TGraphType>(
            this RefactoringContext context, ReferencingNode<IReferToAnExternalReference, TGraphType> input) where TGraphType : IScadGraph
        {
            var refactorableGraph = context.MakeRefactorable(input.Graph);
            var scadNode = refactorableGraph.ById(input.Node.Id);
            return new ReferencingNode<IReferToAnExternalReference, LightWeightGraph>(refactorableGraph, scadNode, (IReferToAnExternalReference) scadNode);
        }

        public static ReferencingNode<IReferToAnInvokable, LightWeightGraph> MakeRefactorable<TGraphType>(
            this RefactoringContext context, ReferencingNode<IReferToAnInvokable, TGraphType> input) where TGraphType : IScadGraph
        {
            var refactorableGraph = context.MakeRefactorable(input.Graph);
            var scadNode = refactorableGraph.ById(input.Node.Id);
            return new ReferencingNode<IReferToAnInvokable, LightWeightGraph>(refactorableGraph, scadNode, (IReferToAnInvokable) scadNode);
        }

        public static ReferencingNode<IReferToAVariable, LightWeightGraph> MakeRefactorable<TGraphType>(
            this RefactoringContext context, ReferencingNode<IReferToAVariable, TGraphType> input) where TGraphType : IScadGraph
        {
            var refactorableGraph = context.MakeRefactorable(input.Graph);
            var scadNode = refactorableGraph.ById(input.Node.Id);
            return new ReferencingNode<IReferToAVariable, LightWeightGraph>(refactorableGraph, scadNode, (IReferToAVariable) scadNode);
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