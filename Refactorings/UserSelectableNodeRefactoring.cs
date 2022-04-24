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

        private static readonly List<IUserSelectableRefactoringFactory> Factories;
        
        protected UserSelectableNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        static UserSelectableNodeRefactoring()
        {
            Factories = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IUserSelectableRefactoringFactory).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IUserSelectableRefactoringFactory>()
                .ToList();
        }
        
        public static IEnumerable<UserSelectableNodeRefactoring> GetApplicable(IScadGraph graph, ScadNode node)
        {
            return Factories
                .SelectMany(it => it.GetRefactorings(graph, node))
                .Where(it => it.IsApplicableToNode)
                .OrderBy(it => it.Order);
        }

        
    }
}