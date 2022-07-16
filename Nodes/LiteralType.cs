using System;

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
                    return LiteralType.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }
    }
}