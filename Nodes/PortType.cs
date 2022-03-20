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
        Any = 8
    }

    public static class PortTypeExt
    {
        public static bool CanConnect(this PortType self, PortType other)
        {
            return self == other ||
                   self == PortType.Any && other != PortType.Flow ||
                   self != PortType.Flow && other == PortType.Any;
        }
    }
}