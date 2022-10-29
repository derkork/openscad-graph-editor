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
        /// A constraint for a number/vector variable which limits its minimum, maximum and step size.
        /// </summary>
        MinStepMax = 1,
        
        /// <summary>
        /// A constraint for a number/vector variable which limits the step size.
        /// </summary>
        Step = 4,
        
        /// <summary>
        /// A constraint for a number variable which limits its maximum.
        /// </summary>
        Max = 5,
        
        /// <summary>
        /// A constraint for a number/string variable which limits its maximum size/length.
        /// </summary>
        MaxLength = 2,
        
        /// <summary>
        /// A constraint which allows only a specific set of values.
        /// </summary>
        Options = 3,
    }
}