using JetBrains.Annotations;
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
            bool isOutput, bool isAutoSet, bool isConnected, IScadLiteralWidget existing = null)
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