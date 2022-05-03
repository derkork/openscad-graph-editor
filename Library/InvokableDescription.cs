using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// Description of an invokable (module or function).
    /// </summary>
    public abstract class InvokableDescription
    {
        /// <summary>
        /// Id of the description.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// The name of the invokable (e.g. function/module name).
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The name that the node representing this should have. If not set, the <see cref="Name"/> will be used to
        /// name the node.
        /// </summary>
        public string NodeName { get; set; } = "";

        /// <summary>
        /// A description of the invokable.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Whether this description originated from an external source.
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Whether this description is for a built-in invokable.
        /// </summary>
        public bool IsBuiltin { get; set; }

        /// <summary>
        /// The parameters of the function/module.
        /// </summary>
        public List<ParameterDescription> Parameters { get; set; } = new List<ParameterDescription>();

        /// <summary>
        /// Returns the display name if set, or the name otherwise.
        /// </summary>
        public string NodeNameOrFallback => NodeName.Length > 0 ? NodeName : Name;

        /// <summary>
        /// Returns true if the given node can be used in this type of invokable.
        /// </summary>
        public abstract bool CanUse(ScadNode node);

        protected void LoadFrom(SavedInvokableDescription savedInvokableDescription)
        {
            Id = savedInvokableDescription.Id;
            Name = savedInvokableDescription.Name;
            NodeName = savedInvokableDescription.NodeName;
            Description = savedInvokableDescription.Description;
            IsExternal = savedInvokableDescription.IsExternal;
            IsBuiltin = savedInvokableDescription.IsBuiltin;

            Parameters = savedInvokableDescription.Parameters
                .Select(it => it.FromSavedState()).ToList();
        }
        
        protected void SaveInto(SavedInvokableDescription savedInvokableDescription)
        {
            savedInvokableDescription.Id = Id;
            savedInvokableDescription.Name = Name;
            savedInvokableDescription.NodeName = NodeName;
            savedInvokableDescription.Description = Description;
            savedInvokableDescription.IsExternal = IsExternal;
            savedInvokableDescription.IsBuiltin = IsBuiltin;

            foreach (var parameterDescription in Parameters)
            {
                savedInvokableDescription.Parameters.Add(parameterDescription.ToSavedState());
            }
        }
    }
}