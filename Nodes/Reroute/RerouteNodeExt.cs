namespace OpenScadGraphEditor.Nodes.Reroute
{
    public static class RerouteNodeExt
    {
        public static bool IsWirelessReroute(this ScadNode node)
        {
            return node is RerouteNode {IsWireless: true};
        }
    }
}