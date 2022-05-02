using GodotExt;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Struct identifying a port of a node. This is to avoid a ton of booleans and if/else constructs. 
    /// </summary>
    public readonly struct PortId
    {
        
        public int Port { get; }
        
        private readonly bool _isInput;
        
        public bool IsInput => IsDefined && _isInput;
        public bool IsOutput => IsDefined && !_isInput;
        
        /// <summary>
        /// The default value of this is false, so if you do a PortId foo = default you will get an undefined port id.
        /// </summary>
        public bool IsDefined { get; }
        
        private PortId(int port, bool isInput)
        {
            GdAssert.That(port >= 0, "Port must be non-negative");
            Port = port;
            _isInput = isInput;
            IsDefined = true;
        }
        
        
        public static PortId Input(int port) => new PortId(port, true);
        public static PortId Output(int port) => new PortId(port, false);

        /// <summary>
        /// The default PortId is undefined and represents no port.
        /// </summary>
        public static readonly PortId None = default;
        
    }
}