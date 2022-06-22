using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

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
            _knownNodeRefactorings = typeof(UserSelectableNodeRefactoring)
                .GetImplementors(typeof(ScadGraph), typeof(ScadNode)).ToList();
        }


        public IEnumerable<UserSelectableNodeRefactoring> GetRefactorings(ScadGraph graph, ScadNode node)
        {
            return _knownNodeRefactorings.CreateInstances<UserSelectableNodeRefactoring>(graph, node);
        }
    }
}