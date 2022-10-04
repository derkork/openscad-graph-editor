﻿using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeVariableTypeRefactoring : Refactoring
    {
        private readonly VariableDescription _description;
        private readonly PortType _newType;

        public ChangeVariableTypeRefactoring(VariableDescription description, PortType newType)
        {
            _description = description;
            _newType = newType;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (_description.TypeHint == _newType)
            {
                return; // Nothing to do
            }


            // first find all nodes that are affected by this
            var affectedNodes = context.Project.FindAllReferencingNodes(_description)
                .ToList();


            _description.TypeHint = _newType;

            // This may lead to some connections being invalid
            // and we also need to update all affected nodes so they can refresh their port types. So we
            // start by walking over all the graphs and find nodes that are affected by this.

            foreach (var nodeReference in affectedNodes)
            {
                var scadNode = nodeReference.Node;
                var graph = nodeReference.Graph;
                var node = nodeReference.NodeAsReference;

                // get all ports representing the return value
                var returnValuePorts = node.GetPortsReferringToVariable().ToList();

                // save all connections that go to any port representing the variable value.
                var affectedConnections = graph.GetAllConnections()
                    .Where(it => it.InvolvesAnyPort(scadNode, returnValuePorts))
                    .ToList();
                
                // and remove them, we'll add the valid ones back later. We do this now because the connection rules
                // check for duplicate connections and would veto a connection that already exists.
                affectedConnections.ForAll(it => graph.RemoveConnection(it));

                // now instruct the node to rebuild its ports using the updated return type
                node.SetupPorts(_description);

                // and rebuild the literal for the affected ports
                returnValuePorts.ForAll(it =>
                {
                    scadNode.DropPortLiteral(it);
                    scadNode.BuildPortLiteral(it);
                });
                
                // now for all the connections we have saved, check if they are still valid and re-add the ones
                // that were not vetoed.
                affectedConnections
                    .Where(it => ConnectionRules.CanConnect(it).Decision != ConnectionRules.OperationRuleDecision.Veto)
                    .ToList()
                    .ForAll(it => graph.AddConnection(it.From.Id, it.FromPort, it.To.Id, it.ToPort));
            }
        }
    }
}