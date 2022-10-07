namespace OpenScadGraphEditor.Library
{
    public enum VariableCustomizerConstraintType
    {
        // Do not change the numbers, otherwise the serialization will break.
        
        /// <summary>
        /// No constraint is active. This is the default value.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// A constraint for a number variable/vector which limits its minimum, maximum and step size.
        /// </summary>
        MinStepMax = 1,
        
        /// <summary>
        /// A constraint for a string variable which limits its maximum size.
        /// </summary>
        MaxLength = 2,
        
        /// <summary>
        /// A constraint which allows only a specific set of values.
        /// </summary>
        Options = 3,
    }
}