using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Default factory that produces <see cref="UserSelectableNodeRefactoring"/> instances.
    /// </summary>
    // this sounds like an effing spring boot class name... 
    [UsedImplicitly]
    public class DefaultUserSelectableRefactoringFactory : IUserSelectableRefactoringFactory
    {
        private readonly List<Type> _knownNodeRefactorings;

        public DefaultUserSelectableRefactoringFactory()
        {
            _knownNodeRefactorings = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(UserSelectableNodeRefactoring).IsAssignableFrom(t) && !t.IsAbstract)
                .Where(t => t.GetConstructor(new[] {typeof(ScadGraph), typeof(ScadNode)}) != null)
                .ToList();
        }


        public IEnumerable<UserSelectableNodeRefactoring> GetRefactorings(ScadGraph graph, ScadNode node)
        {
            return _knownNodeRefactorings
                .Select(it => Activator.CreateInstance(it, graph, node))
                .Cast<UserSelectableNodeRefactoring>();
        }
    }
}