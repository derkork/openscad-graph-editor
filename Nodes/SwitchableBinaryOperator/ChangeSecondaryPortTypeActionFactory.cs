using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    [UsedImplicitly]
    public class ChangeSecondaryPortTypeActionFactory : IEditorActionFactory
    {
        public IEnumerable<IEditorAction> CreateActions()
        {
            // get all port types which are expression types and create an action for each of them
            return Enum.GetValues(typeof(PortType))
                .Cast<PortType>()
                .Where(it => it.IsExpressionType())
                .Select(it => new ChangeSecondaryPortTypeAction(it));
        }
    }
}