using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public abstract class UserSelectableNodeRefactoring : NodeRefactoring
    {
        
        public abstract string Title { get; }
        
        public abstract bool IsApplicableToNode { get; }
        
        public abstract int Order { get; }

        private static readonly List<Type> KnownNodeRefactorings;
        
        protected UserSelectableNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        static UserSelectableNodeRefactoring()
        {
            KnownNodeRefactorings = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(UserSelectableNodeRefactoring).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
        }
        
        public static List<UserSelectableNodeRefactoring> GetApplicable(IScadGraph graph, ScadNode node)
        {
            return KnownNodeRefactorings
                .Select(it => Activator.CreateInstance(it, graph, node))
                .Cast<UserSelectableNodeRefactoring>()
                .Where(it => it.IsApplicableToNode)
                .OrderBy(it => it.Order)
                .ToList();
        }

        
    }
}