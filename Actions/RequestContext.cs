using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Actions
{
    public class RequestContext
    {
        public static RequestContext ForPosition(ScadGraph source, Vector2 position)
        {
            return new RequestContext(source, position);
        }

        public static RequestContext FromPort(ScadGraph source, Vector2 position, ScadNode node, int port)
        {
            return new RequestContext(source, position, node, PortId.Output(port));
        }
        public static RequestContext ToPort(ScadGraph source, Vector2 position, ScadNode node, int port)
        {
            return new RequestContext(source, position, node, PortId.Input(port));
        }

        public static RequestContext ForNode(ScadGraph source, Vector2 position, ScadNode node)
        {
            return new RequestContext(source, position, node);
        }

        public static RequestContext ForInvokableDescription(InvokableDescription description)
        {
            return new RequestContext(invokableDescription:description);
        }
        
        public static RequestContext ForVariableDescription(VariableDescription description)
        {
            return new RequestContext(variableDescription:description);
        }
        
        public static RequestContext ForExternalReference(ExternalReference reference)
        {
            return new RequestContext(externalReference:reference);
        }
        
        private RequestContext(ScadGraph graph = default, 
            Vector2 position = default, 
            ScadNode node = default, 
            PortId portId = default, 
            InvokableDescription invokableDescription = default,
            VariableDescription variableDescription = default,
            ExternalReference externalReference = default
            )
        {
            _graph = graph;
            _position = position;
            _node = node;
            _port = portId;
            _invokableDescription = invokableDescription;
            _variableDescription = variableDescription;
            _externalReference = externalReference;
        }

        private readonly ScadGraph _graph;
        private readonly Vector2 _position; 
        private readonly ScadNode _node;
        private readonly PortId _port;
        private readonly InvokableDescription _invokableDescription;
        private readonly VariableDescription _variableDescription;
        private readonly ExternalReference _externalReference;

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
        
        
        public bool TryGetPosition(out ScadGraph graph, out Vector2 position)
        {
            if (_graph == null)
            {
                graph = default;
                position = default;
                return false;
            }

            graph = _graph;
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