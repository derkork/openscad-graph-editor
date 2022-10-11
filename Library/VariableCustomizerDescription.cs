using System;
using Godot.Collections;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// Base class for variable customizer descriptions.
    /// </summary>
    public class VariableCustomizerDescription
    {
        /// <summary>
        /// The constraint type that is applied on the variable in the customizer.
        /// </summary>
        public VariableCustomizerConstraintType ConstraintType { get; set; } = VariableCustomizerConstraintType.None;

        /// <summary>
        /// The name of the tab in which the variable is displayed. If empty, the variable is displayed in the default tab.
        /// </summary>
        public string Tab { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MinStepMax"/>, this property contains
        /// the minimum value of the variable.
        /// </summary>
        public string Min { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MinStepMax"/>, this property contains
        /// the step value of the variable.
        /// </summary>
        public string Step { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MinStepMax"/>, this property contains
        /// the maximum value of the variable.
        /// </summary>
        public string Max { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MaxLength"/>, this property contains
        /// the maximum length of the string.
        /// </summary>
        public string MaxLength { get; set; } = "";

        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.Options"/>, this property contains
        /// the value and display name of the options.
        /// </summary>
        // we use a Godot dictionary here because it preserves insert order which is useful in this case
        public Dictionary<IScadLiteral, StringLiteral> ValueLabelPairs { get; set; } = new Dictionary<IScadLiteral, StringLiteral>();

        /// <summary>
        /// Loads the variable customizer description from the specified saved data.
        /// </summary>
        public void LoadFrom(VariableDescription owner, SavedVariableCustomizerDescription saved)
        {
            ConstraintType = saved.ConstraintType;
            Tab = saved.Tab;
            Min = saved.Min;
            Step = saved.Step;
            Max = saved.Max;
            MaxLength = saved.MaxLength;
            ValueLabelPairs = new Dictionary<IScadLiteral, StringLiteral>();
            foreach (var pair in saved.ValueLabelPairs)
            {
                try
                {
                    // depending on the variable type the value is either a string or a number
                    IScadLiteral value;
                    if (owner.TypeHint == PortType.String)
                    {
                        value = new StringLiteral(pair.Key);
                    }
                    else
                    {
                        value = new NumberLiteral(pair.Key.SafeParse());
                    }

                    // the label is always a string
                    var label = new StringLiteral(pair.Value);

                    ValueLabelPairs.Add(value, label);
                }
                catch (Exception e)
                {
                    throw new BrokenFileException(
                        $"Broken literal value for variable {owner.Name} ({pair.Key} -> {pair.Value})", e);
                }
            }
        }


        /// <summary>
        /// Saves the variable customizer description to the specified saved data.
        /// </summary>
        public void SaveInto(SavedVariableCustomizerDescription saved)
        {
            saved.ConstraintType = ConstraintType;
            saved.Tab = Tab;
            saved.Min = Min;
            saved.Step = Step;
            saved.Max = Max;
            saved.MaxLength = MaxLength;
            saved.ValueLabelPairs = new Dictionary<string, string>();
            foreach (var pair in ValueLabelPairs)
            {
                saved.ValueLabelPairs.Add(pair.Key.SerializedValue, pair.Value.SerializedValue);
            }
            
        }
    }
}