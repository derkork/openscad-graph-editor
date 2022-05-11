using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeInvokableParameterTypeRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private readonly string _parameterName;
        private readonly PortType _newPortType;

        public ChangeInvokableParameterTypeRefactoring(InvokableDescription description, string parameterName,
            PortType newPortType)
        {
            _description = description;
            _parameterName = parameterName;
            _newPortType = newPortType;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var parameterIndex = _description.Parameters
                .Indices()
                .First(i => _description.Parameters[i].Name == _parameterName);

            if (_description.Parameters[parameterIndex].TypeHint == _newPortType)
            {
                return; // nothing to do.
            }

            // find all nodes that refer to this description
            var nodes = context.Project.FindAllReferencingNodes(_description)
                .ToList(); // ensure this is all done before we start modifying the graphs

            // only change the type AFTER we have made everything refactorable, otherwise the internal state of the
            // nodes is outdated
            _description.Parameters[parameterIndex].TypeHint = _newPortType;

            // This may lead to some connections being invalid
            // and we also need to update all affected nodes so they can refresh their port types.
            foreach (var referencingNode in nodes)
            {
                var affectedConnections = new List<ScadConnection>();
                var scadNode = referencingNode.Node;
                var node = referencingNode.NodeAsReference;

                // check which input port is responsible for the given parameter.
                var inputPort = node.GetParameterInputPort(parameterIndex);
                var graph = referencingNode.Graph;

                if (inputPort != -1)
                {
                    affectedConnections.AddRange(graph.GetAllConnections().Where(it => it.IsTo(scadNode, inputPort)));
                }

                // same for the output ports
                var outputPort = node.GetParameterOutputPort(parameterIndex);
                if (outputPort != -1)
                {
                    affectedConnections.AddRange(graph.GetAllConnections()
                        .Where(it => it.IsFrom(scadNode, outputPort)));
                }

                // now instruct the node to rebuild its ports using the updated parameter type
                node.SetupPorts(_description);

                // and rebuild the literal for the affected ports
                if (inputPort != -1)
                {
                    var portId = PortId.Input(inputPort);
                    // try to keep the "is set" state. as it was before
                    var wasSet = scadNode.TryGetLiteral(portId, out var literal) && literal.IsSet;
                    
                    scadNode.DropPortLiteral(portId);
                    scadNode.BuildPortLiteral(portId);
                    
                    if (scadNode.TryGetLiteral(portId, out var newLiteral))
                    {
                        newLiteral.IsSet = wasSet;
                    }
                }

                if (outputPort != -1)
                {
                    var portId = PortId.Output(outputPort);
                    // try to keep the "is set" state. as it was before
                    var wasSet = scadNode.TryGetLiteral(portId, out var literal) && literal.IsSet;
                    scadNode.DropPortLiteral(portId);
                    scadNode.BuildPortLiteral(portId);
                    if (scadNode.TryGetLiteral(portId, out var newLiteral))
                    {
                        newLiteral.IsSet = wasSet;
                    }
                }


                // now for all the connections we have saved, check if they are still valid.
                affectedConnections
                    .Where(it => ConnectionRules.CanConnect(it).Decision == ConnectionRules.OperationRuleDecision.Veto)
                    .ToList()
                    // and remove the ones that are vetoed.
                    .ForAll(it => graph.RemoveConnection(it));
                
                // finally if the parameter was optional before but the new parameter type does not support literals
                // then we need to make the parameter mandatory
                if (_description.Parameters[parameterIndex].IsOptional && _newPortType.GetMatchingLiteralType() == LiteralType.None)
                {
                    context.PerformRefactoring(new ChangeInvokableParameterOptionalStateRefactoring(_description, parameterIndex, false));
                }
            }
        }
    }
}