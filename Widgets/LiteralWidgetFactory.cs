﻿using System;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public static class LiteralWidgetFactory
    {
        /// <summary>
        /// Builds a new widget for the given literal. Can be given an existing widget which will be re-used
        /// in case it fit the literal. Returns the created or re-used widget or null if no widget could be
        /// created.
        /// </summary>
        public static IScadLiteralWidget BuildWidget([CanBeNull] this IScadLiteral literal,
            bool isOutput, bool isAutoSet, bool isConnected, IScadLiteralWidget existing = null, 
            VariableCustomizerDescription customizerDescription = null)
        {
            IScadLiteralWidget result = null;

            switch (literal)
            {
                case BooleanLiteral booleanLiteral:
                    if (!(existing is BooleanEdit booleanEdit))
                    {
                        booleanEdit = Prefabs.New<BooleanEdit>();
                    }

                    booleanEdit.BindTo(booleanLiteral, isOutput, isAutoSet, isConnected);
                    result = booleanEdit;
                    break;

                case NumberLiteral numberLiteral:
                    if (customizerDescription != null)
                    {
                        if (customizerDescription.ConstraintType == VariableCustomizerConstraintType.MinStepMax)
                        {
                            // render a slider
                            if (!(existing is NumberSlider numberSlider))
                            {
                                numberSlider = Prefabs.New<NumberSlider>();
                            }

                            numberSlider.Min = customizerDescription.Min.SafeParse(0);
                            numberSlider.Max = customizerDescription.Max.SafeParse(100);
                            numberSlider.Step = customizerDescription.Step.SafeParse(1);
                            numberSlider.BindTo(numberLiteral, isOutput, isAutoSet, isConnected);
                            result = numberSlider;
                            break;
                        }

                        if (customizerDescription.ConstraintType == VariableCustomizerConstraintType.Step)
                        {
                            // render a spin edit
                            if (!(existing is NumberSpinEdit numberSpinEdit))
                            {
                                numberSpinEdit = Prefabs.New<NumberSpinEdit>();
                            }

                            numberSpinEdit.Min = null;
                            numberSpinEdit.Max = null;
                            numberSpinEdit.Step = customizerDescription.Step.SafeParse(1);
                            numberSpinEdit.BindTo(numberLiteral, isOutput, isAutoSet, isConnected);
                            result = numberSpinEdit;
                            break;
                        }
                        
                        if (customizerDescription.ConstraintType == VariableCustomizerConstraintType.Max)
                        {
                            // render a spin edit
                            if (!(existing is NumberSpinEdit numberSpinEdit))
                            {
                                numberSpinEdit = Prefabs.New<NumberSpinEdit>();
                            }
                            
                            numberSpinEdit.Min = null;
                            numberSpinEdit.Step = 1;
                            numberSpinEdit.Max = customizerDescription.Max.SafeParse(100);
                            numberSpinEdit.BindTo(numberLiteral, isOutput, isAutoSet, isConnected);
                            result = numberSpinEdit;
                            break;
                        }

                        if (customizerDescription.ConstraintType == VariableCustomizerConstraintType.Options)
                        {
                            // render a select box
                            if (!(existing is SelectBox selectBox))
                            {
                                selectBox = Prefabs.New<SelectBox>();
                            }
                            
                            selectBox.Options = customizerDescription.ValueLabelPairs;
                            selectBox.BindTo(numberLiteral, isOutput, isAutoSet, isConnected);
                            result = selectBox;
                            break;
                        }
                    }
                    
                    if (!(existing is NumberEdit numberEdit))
                    {
                        numberEdit = Prefabs.New<NumberEdit>();
                    }

                    numberEdit.BindTo(numberLiteral, isOutput, isAutoSet, isConnected);
                    result = numberEdit;
                    break;

                case StringLiteral stringLiteral:
                    if (!(existing is StringEdit stringEdit))
                    {
                        stringEdit = Prefabs.New<StringEdit>();
                    }

                    if (customizerDescription?.ConstraintType == VariableCustomizerConstraintType.MaxLength)
                    {
                        stringEdit.MaxLength = (int) customizerDescription.MaxLength.SafeParse(0);
                    }
                    
                    stringEdit.BindTo(stringLiteral, isOutput, isAutoSet, isConnected);
                    result = stringEdit;
                    break;

                case NameLiteral nameLiteral:
                    if (!(existing is NameEdit nameEdit))
                    {
                        nameEdit = Prefabs.New<NameEdit>();
                    }

                    nameEdit.BindTo(nameLiteral, isOutput, isAutoSet, isConnected);
                    result = nameEdit;
                    break;

                case Vector3Literal vector3Literal:
                    if (!(existing is Vector3Edit vector3Edit))
                    {
                        vector3Edit = Prefabs.New<Vector3Edit>();
                    }

                    vector3Edit.BindTo(vector3Literal, isOutput, isAutoSet, isConnected);
                    result = vector3Edit;
                    break;

                case Vector2Literal vector2Literal:
                    if (!(existing is Vector2Edit vector2Edit))
                    {
                        vector2Edit = Prefabs.New<Vector2Edit>();
                    }

                    vector2Edit.BindTo(vector2Literal, isOutput, isAutoSet, isConnected);
                    result = vector2Edit;
                    break;
            }

            return result;
        }
    }
}