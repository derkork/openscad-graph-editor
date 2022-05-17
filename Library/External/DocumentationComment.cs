using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// This holds the contents of a documentation comment.
    /// </summary>
    public class DocumentationComment
    {
        private class ParameterDescription
        {
            public readonly string Name;
            public readonly string Description;
            public readonly PortType TypeHint;

            public ParameterDescription(string name = "", string description = "", PortType typeHint = PortType.None)
            {
                Name = name;
                Description = description;
                TypeHint = typeHint;
            }
        }

        public string Summary { private get; set; } = "";
        public string ReturnValueDescription { private get; set; } = "";
        public PortType ReturnValueTypeHint { private get; set; } = PortType.None;
        
        private readonly List<ParameterDescription> _parameters = new List<ParameterDescription>();
        
        
        /// <summary>
        /// Applies the contents of the documentation comment to the given invokable description.
        /// </summary>
        public void ApplyTo(InvokableDescription description)
        {
            description.Description = Summary;
            foreach (var parameter in description.Parameters)
            {
                var parameterDescription = _parameters.LastOrDefault(it => it.Name == parameter.Name);
                if (parameterDescription == null)
                {
                    // no matching parameter description found in comment, skip over to the next one.
                    continue;
                }

                if (parameterDescription.TypeHint != PortType.None)
                {
                    // type hint from the comment always wins over any inferred type hint.
                    parameter.TypeHint = parameterDescription.TypeHint;
                }
                parameter.Description = parameterDescription.Description;
            }

            if (description is FunctionDescription functionDescription)
            {
                if (ReturnValueTypeHint != PortType.None)
                {
                    // also a return type hint from the comment always wins over any inferred type hint.
                    functionDescription.ReturnTypeHint = ReturnValueTypeHint;
                }
                functionDescription.ReturnValueDescription = ReturnValueDescription;
            }
        }
        
        public void AddParameter(string name, string description = "", PortType typeHint = PortType.None)
        {
            _parameters.Add(new ParameterDescription(name, description, typeHint));
        }
    }
}