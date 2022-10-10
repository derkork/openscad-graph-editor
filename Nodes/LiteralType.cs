using System;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// The known literal types
    /// </summary>
    public enum LiteralType
    {
        None,
        String,
        Number,
        Boolean,
        Vector2,
        Vector3,
        Name
    }

    public static class LiteralTypeExt
    {
        public static LiteralType GetMatchingLiteralType(this PortType portType)
        {
            switch (portType)
            {
                case PortType.Boolean:
                    return LiteralType.Boolean;
                case PortType.Number:
                    return LiteralType.Number;  
                case PortType.Vector2:
                    return LiteralType.Vector2;
                case PortType.Vector3:
                    return LiteralType.Vector3;
                case PortType.String:
                    return LiteralType.String;
                case PortType.Vector:
                case PortType.Any:
                case PortType.Geometry:
                case PortType.Reroute:
                case PortType.None:
                    return LiteralType.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, "unknown port type");
            }
        }
        
        /// <summary>
        /// Builds a literal for this literal type. Returns the built literal or null if the literal type is "none".
        /// </summary>
        [CanBeNull]
        public static IScadLiteral BuildLiteral(this LiteralType literalType, string serializedValue = null)
        {
            IScadLiteral result;
            switch (literalType)
            {
                case LiteralType.Boolean:
                    result = new BooleanLiteral(false);
                    break;
                case LiteralType.Number:
                    result =  new NumberLiteral(0);
                    break;
                case LiteralType.Vector2:
                    result =  new Vector2Literal();
                    break;
                case LiteralType.Vector3:
                    result =  new Vector3Literal();
                    break;
                case LiteralType.String:
                    result =  new StringLiteral("");
                    break;
                case LiteralType.Name:
                    result =  new NameLiteral("");
                    break;
                case LiteralType.None:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(literalType), $"unknown literal type {literalType}");
            }

            if (serializedValue != null)
            {
                result.SerializedValue = serializedValue;
            }

            return result;
        }
    }
}