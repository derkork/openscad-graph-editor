namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Struct identifying a port of a node. This is to avoid a ton of booleans and if/else constructs. 
    /// </summary>
    public readonly struct PortId
    {
        public int Port { get; }
        public bool IsInput { get; }
        public bool IsOutput => !IsInput;
        
        public PortId(int port, bool isInput)
        {
            Port = port;
            IsInput = isInput;
        }
        
        public static PortId Input(int port) => new PortId(port, true);
        public static PortId Output(int port) => new PortId(port, false);
    }
}