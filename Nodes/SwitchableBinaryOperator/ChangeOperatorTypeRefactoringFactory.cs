using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    [UsedImplicitly]
    public class ChangeOperatorTypeRefactoringFactory : IUserSelectableRefactoringFactory
    {
        public IEnumerable<UserSelectableNodeRefactoring> GetRefactorings(ScadGraph graph, ScadNode node)
        {
            if (!(node is SwitchableBinaryOperator))
            {
                return Enumerable.Empty<UserSelectableNodeRefactoring>();
            }

            return typeof(SwitchableBinaryOperator).GetImplementors()
                .Select(it => new ChangeOperatorTypeRefactoring(graph, node, it));
        }
    }
}