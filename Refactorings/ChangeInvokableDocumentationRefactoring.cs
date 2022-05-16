using System.Collections.Generic;
using GodotExt;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring for modifying the documentation of an Invokable.
    /// </summary>
    public class ChangeInvokableDocumentationRefactoring : Refactoring
    {
        private readonly InvokableDescription _invokableDescription;
        private readonly string _description;
        private readonly string _returnValueDescription;
        private readonly List<string> _parameterDescriptions;

        public ChangeInvokableDocumentationRefactoring(FunctionDescription functionDescription,
            string description, string returnValueDescription, List<string> parameterDescriptions)
        {
            _invokableDescription = functionDescription;
            _description = description;
            _returnValueDescription = returnValueDescription;
            _parameterDescriptions = parameterDescriptions;
            GdAssert.That(_parameterDescriptions.Count == functionDescription.Parameters.Count, "Parameter count mismatch");
        }

        public ChangeInvokableDocumentationRefactoring(ModuleDescription moduleDescription,
            string description, List<string> parameterDescriptions)
        {
            _invokableDescription = moduleDescription;
            _description = description;
            _parameterDescriptions = parameterDescriptions;
            GdAssert.That(_parameterDescriptions.Count == moduleDescription.Parameters.Count, "Parameter count mismatch");
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            _invokableDescription.Description = _description;
            if (_invokableDescription is FunctionDescription functionDescription)
            {
                functionDescription.ReturnValueDescription = _returnValueDescription;
            }
            
            for (var i = 0; i < _parameterDescriptions.Count; i++)
            {
                _invokableDescription.Parameters[i].Description = _parameterDescriptions[i];
            }
        }
    }
}