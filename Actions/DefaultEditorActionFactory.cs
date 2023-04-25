using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Actions
{
    /// <summary>
    /// Default factory for editor actions. This automatically creates all editor actions which have
    /// a parameterless constructor.
    /// </summary>
    [UsedImplicitly]
    public class DefaultEditorActionFactory : IEditorActionFactory
    {
        public IEnumerable<IEditorAction> CreateActions() =>
            typeof(IEditorAction)
                .GetImplementors()
                .CreateInstances<IEditorAction>();
    }
}