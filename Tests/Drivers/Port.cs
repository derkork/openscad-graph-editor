using System;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public readonly struct Port
    {
        public int PortIndex { get; }
        
        private readonly bool _isInput;
        
        public bool IsInput => IsDefined && _isInput;
        public bool IsOutput => IsDefined && !_isInput;
        
        /// <summary>
        /// The default value of this is false, so if you do a Port foo = default you will get an undefined port id.
        /// </summary>
        public bool IsDefined { get; }
        
        private Port(int port, bool isInput)
        {
            if (port < 0)
            {
                throw new ArgumentException("Port index must be greater than or equal to zero.");
            }
            PortIndex = port;
            _isInput = isInput;
            IsDefined = true;
        }
        
        
        public static Port Input(int port) => new Port(port, true);
        public static Port Output(int port) => new Port(port, false);

        /// <summary>
        /// The default Port is undefined and represents no port.
        /// </summary>
        public static readonly Port None = default;
    }
}