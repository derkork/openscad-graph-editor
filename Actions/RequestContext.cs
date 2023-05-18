using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Actions
{
    public readonly struct RequestContext
    {
        public static RequestContext ForOutputPort(ScadGraph source, Vector2 position, ScadNode node, int port)
        {
            return new RequestContext().WithNodePort(source, node, PortId.Output(port)).WithPosition(position);
        }

        public static RequestContext ForInputPort(ScadGraph source, Vector2 position, ScadNode node, int port)
        {
            return new RequestContext().WithNodePort(source, node, PortId.Input(port)).WithPosition(position);
        }

        public static RequestContext ForNode(ScadGraph source, ScadNode node, Vector2 position)
        {
            return new RequestContext().WithNode(source, node).WithPosition(position);
        }

        public static RequestContext ForInvokableDescription(InvokableDescription description)
        {
            return new RequestContext().WithInvokableDescription(description);
        }

        public static RequestContext ForVariableDescription(VariableDescription description)
        {
            return new RequestContext().WithVariableDescription(description);
        }

        public static RequestContext ForExternalReference(ExternalReference reference)
        {
            return new RequestContext().WithExternalReference(reference);
        }


        private readonly ScadGraph _graph;
        private readonly Vector2 _position;
        private readonly bool _hasPosition;
        private readonly ScadNode _node;
        private readonly PortId _port;
        private readonly InvokableDescription _invokableDescription;
        private readonly VariableDescription _variableDescription;
        private readonly ExternalReference _externalReference;

        private RequestContext(ScadGraph graph, Vector2 position, bool hasPosition,
            ScadNode node, PortId port, InvokableDescription invokableDescription,
            VariableDescription variableDescription, ExternalReference externalReference)
        {
            _graph = graph;
            _position = position;
            _hasPosition = hasPosition;
            _node = node;
            _port = port;
            _invokableDescription = invokableDescription;
            _variableDescription = variableDescription;
            _externalReference = externalReference;
        }
        
        public bool IsEmpty => _graph == null && _node == null && _invokableDescription == null &&
                               _variableDescription == null && _externalReference == null;

        public RequestContext WithGraph(ScadGraph graph)
        {
            return new RequestContext(graph, _position, _hasPosition, _node, _port, _invokableDescription,
                _variableDescription, _externalReference);
        }

        public RequestContext WithPosition(Vector2 position)
        {
            return new RequestContext(_graph, position, true, _node, _port, _invokableDescription, _variableDescription,
                _externalReference);
        }

        public RequestContext WithNode(ScadGraph graph, ScadNode node)
        {
            return new RequestContext(graph, _position, _hasPosition, node, _port, _invokableDescription,
                _variableDescription, _externalReference);
        }

        public RequestContext WithNodePort(ScadGraph graph, ScadNode node, PortId port)
        {
            return new RequestContext(graph, _position, _hasPosition, node, port, _invokableDescription,
                _variableDescription, _externalReference);
        }

        public RequestContext WithInvokableDescription(InvokableDescription description)
        {
            return new RequestContext(_graph, _position, _hasPosition, _node, _port, description,
                _variableDescription, _externalReference);
        }

        public RequestContext WithVariableDescription(VariableDescription description)
        {
            return new RequestContext(_graph, _position, _hasPosition, _node, _port, _invokableDescription,
                description, _externalReference);
        }

        public RequestContext WithExternalReference(ExternalReference reference)
        {
            return new RequestContext(_graph, _position, _hasPosition, _node, _port, _invokableDescription,
                _variableDescription, reference);
        }


        public bool TryGetNodeAndPort(out ScadGraph graph, out ScadNode node, out PortId port)
        {
            if (_graph == null || _node == null || !_port.IsDefined)
            {
                graph = default;
                node = default;
                port = default;
                return false;
            }

            graph = _graph;
            node = _node;
            port = _port;
            return true;
        }

        public bool TryGetNode(out ScadGraph graph, out ScadNode node)
        {
            if (_graph == null || _node == null)
            {
                graph = default;
                node = default;
                return false;
            }

            graph = _graph;
            node = _node;
            return true;
        }


        public bool TryGetPositionInGraph(out ScadGraph graph, out Vector2 position)
        {
            if (_graph == null || !_hasPosition)
            {
                graph = default;
                position = default;
                return false;
            }

            graph = _graph;
            position = _position;
            return true;
        }

        public bool TryGetPosition(out Vector2 position)
        {
            if (!_hasPosition)
            {
                position = default;
                return false;
            }

            position = _position;
            return true;
        }


        public bool TryGetInvokableDescription(out InvokableDescription description)
        {
            if (_invokableDescription == null)
            {
                // if we have a node and this node is a reference to an invokable, we can use that
                if (_node is IReferToAnInvokable invokableReference)
                {
                    description = invokableReference.InvokableDescription;
                    return true;
                }

                description = default;
                return false;
            }

            description = _invokableDescription;
            return true;
        }

        public bool TryGetVariableDescription(out VariableDescription description)
        {
            if (_variableDescription == null)
            {
                // if we have a node and this node is a reference to a variable, we can use that
                if (_node is IReferToAVariable variableReference)
                {
                    description = variableReference.VariableDescription;
                    return true;
                }

                description = default;
                return false;
            }

            description = _variableDescription;
            return true;
        }

        public bool TryGetExternalReference(out ExternalReference reference)
        {
            if (_externalReference == null)
            {
                reference = default;
                return false;
            }

            reference = _externalReference;
            return true;
        }
    }
}