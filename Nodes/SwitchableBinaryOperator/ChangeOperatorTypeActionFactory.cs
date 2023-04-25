using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    [UsedImplicitly]
    public class ChangeOperatorTypeActionFactory : IEditorActionFactory
    {

        public IEnumerable<IEditorAction> CreateActions()
        {
            return typeof(SwitchableBinaryOperator)
                .GetImplementors()
                .Select(it => new ChangeOperatorTypeAction(it));
        }
    }
}