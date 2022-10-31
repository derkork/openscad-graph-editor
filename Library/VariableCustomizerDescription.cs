using System;
using System.Collections.Generic;
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
        /// we use list/tuple to preserve the order of the options
        public List<(IScadLiteral Value, StringLiteral Label)> ValueLabelPairs { get; } = new List<(IScadLiteral Value, StringLiteral Label)>();

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
            ValueLabelPairs.Clear();
            for (var i = 0; i < saved.OptionValues.Count; i++)
            {
                var optionValue = saved.OptionValues[i];
                var optionLabel = saved.OptionLabels[i];
                try
                {
                    // depending on the variable type the value is either a string or a number
                    IScadLiteral value;
                    if (owner.TypeHint == PortType.String)
                    {
                        value = new StringLiteral(optionValue);
                    }
                    else
                    {
                        value = new NumberLiteral(optionValue.SafeParse());
                    }

                    // the label is always a string
                    var label = new StringLiteral(optionLabel);

                    ValueLabelPairs.Add((value, label));
                }
                catch (Exception e)
                {
                    throw new BrokenFileException(
                        $"Broken literal value for variable {owner.Name} ({optionValue} -> {optionLabel})", e);
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
            foreach (var pair in ValueLabelPairs)
            {
                saved.OptionValues.Add(pair.Value.SerializedValue);
                saved.OptionLabels.Add(pair.Label.SerializedValue);
            }
            
        }
    }
}