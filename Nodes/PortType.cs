using System;

namespace OpenScadGraphEditor.Nodes
{
    public enum PortType
    {
        // do not change the numbers, otherwise saved graphs will break!
        Flow = 1,
        Boolean = 2,
        Number = 3,
        Vector3 = 4,
        Array = 5,
        String = 6,
        Any = 8,
        Reroute = 9
    }

    public static class PortTypeExt
    {
        public static bool IsExpressionType(this PortType self)
        {
            switch (self)
            {
                case PortType.Boolean:
                case PortType.Number:
                case PortType.Vector3:
                case PortType.Array:
                case PortType.String:
                case PortType.Any:
                    return true;
                case PortType.Flow:
                case PortType.Reroute:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(self), self, null);
            } 
        }
    }
}