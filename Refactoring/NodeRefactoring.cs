using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactoring
{
    public abstract class NodeRefactoring : Refactoring
    {
        public abstract string Title { get; }
        
        public abstract bool Applies { get; }
        
        protected IScadGraph Holder { get; }
        protected ScadNode Node { get; }

        public NodeRefactoring(IScadGraph holder, ScadNode node)
        {
            Holder = holder;
            Node = node;
        }


        private static readonly List<Type> KnownNodeRefactorings;
        
        static NodeRefactoring()
        {
            KnownNodeRefactorings = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(NodeRefactoring).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
        }
        
        public static List<NodeRefactoring> GetApplicable(IScadGraph graph, ScadNode node)
        {
            return KnownNodeRefactorings
                .Select(it => Activator.CreateInstance(it, graph, node))
                .Cast<NodeRefactoring>()
                .Where(it => it.Applies)
                .ToList();
        }
    }
}