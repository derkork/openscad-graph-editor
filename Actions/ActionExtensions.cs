using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Actions
{
    /// <summary>
    /// Extensions which allow to quickly check for an item type to determine whether an action would use it.
    /// </summary>
    public static class ActionExtensions
    {
        public static bool IsInvokable(this RequestContext item, out InvokableDescription result)
        {
            if (item.TryGetInvokableDescription(out result) && !(result is MainModuleDescription))
            {
                return true;
            }

            // check if it is a node referring to an invokable
            if (!item.TryGetNode(out _, out var node) || !(node is IReferToAnInvokable referToAnInvokable))
            {
                return false;
            }

            if (referToAnInvokable.InvokableDescription is MainModuleDescription)
            {
                return false;
            }

            result = referToAnInvokable.InvokableDescription;
            return true;

        }

        public static bool IsEditableInvokable(this RequestContext item, IEditorContext context,
            out InvokableDescription result)
        {
            result = default;

            if (!item.IsInvokable(out var invokableDescription))
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

        public static bool IsVariable(this RequestContext item, out VariableDescription result)
        {
            if (item.TryGetVariableDescription(out result))
            {
                return true;
            }

            // check if it is a node referring to a variable
            if (item.TryGetNode(out _, out var node) && node is IReferToAVariable referToAVariable)
            {
                result = referToAVariable.VariableDescription;
                return true;
            }

            return false;
        }

        public static bool IsEditableVariable(this RequestContext item, IEditorContext context,
            out VariableDescription result)
        {
            result = default;
            if (!item.IsVariable(out var variableDescription))
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

        public static bool IsEntryPoint(this RequestContext item)
        {
            return item.TryGetNode(out _, out var node) && node is EntryPoint;
        }
    }
}