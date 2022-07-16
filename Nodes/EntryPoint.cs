using System;
using System.Linq;
using System.Text;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class EntryPoint : ScadNode, ICannotBeDeleted, ICannotBeCreated
    {
        protected string RenderDocumentationComment(InvokableDescription invokableDescription)
        {
            var mainDescription = invokableDescription.Description;
            var parameters = invokableDescription.Parameters;

            var result = new StringBuilder();
            result.Append(mainDescription.WordWrap(75));

            if (parameters.Count > 0)
            {
                result.Append("\n");
                result.Append(
                    parameters
                        .Select(it => $"@param {it.Name} {ToDocTypeHint(it.TypeHint)} {EscapeForComment(it.Description)}")
                        .Select(it => it.WordWrap(75))
                        .JoinToString("")
                    );
            }

            if (invokableDescription is FunctionDescription functionDescription)
            {
                result.Append("\n");
                var returnValueDescription = $"@return {ToDocTypeHint(functionDescription.ReturnTypeHint)} {EscapeForComment(functionDescription.ReturnValueDescription)}";
                returnValueDescription = returnValueDescription.WordWrap(75);
                // strip trailing newline
                returnValueDescription = returnValueDescription.Substring(0, returnValueDescription.Length - 1);
                result.Append(returnValueDescription);
            }
            
            return "/**\n" + result.ToString().PrefixLines(" * ") + "\n */";
        }

        
        private string EscapeForComment(string description)
        {
            return description.Replace("*/", "").Replace("\r\n", "\n");
        }
        private string ToDocTypeHint(PortType portType)
        {
            switch (portType)
            {
                case PortType.Reroute:
                case PortType.Geometry:
                    return "";
                case PortType.Boolean:
                    return "[boolean]";
                case PortType.Number:
                    return "[number]";
                case PortType.Vector3:
                    return "[vector3]";
                case PortType.Vector:
                    return "[array]";
                case PortType.String:
                    return "[string]";
                case PortType.Any:
                    return "[any]";
                case PortType.Vector2:
                    return "[vector2]";
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            } 
        }

        public abstract string RenderEntryPoint(string content);
    }
}