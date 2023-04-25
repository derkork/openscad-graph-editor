using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Actions
{
    /// <summary>
    /// Extensions which allow to quickly check for an item type to determine whether an action would use it.
    /// </summary>
    public static class ActionExtensions
    {
        public static bool IsEditableInvokable(this RequestContext item, IEditorContext context, out InvokableDescription result)
        {
            result = default;

            if (!item.TryGetInvokableDescription(out var invokableDescription) || invokableDescription is MainModuleDescription)
            {
                return false;
            }
            if (!context.CurrentProject.IsDefinedInThisProject(invokableDescription))
            {
                return false;
            }

            result = invokableDescription;
            return true;
        }
        
        public static bool IsEditableVariable(this RequestContext item, IEditorContext context, out VariableDescription result)
        {
            result = default;
            if (!item.TryGetVariableDescription(out var variableDescription))
            {
                return false;
            }

            if (!context.CurrentProject.IsDefinedInThisProject(variableDescription))
            {
                return false;
            }

            result = variableDescription;
            return true;
        }

        public static bool IsDirectExternalReference(this RequestContext item, out ExternalReference result)
        {
            result = default;
            if (!item.TryGetExternalReference(out var externalReference) || externalReference.IsTransitive)
            {
                return false;
            }
         
            result = externalReference;
            return false;
        }
    }
}