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

        public ChangeInvokableParameterTypeRefactoring(InvokableDescription description, string parameterName, PortType newPortType)
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
            
            
            // first find all graphs that are affected by this and make them refactorable
            var graphs = context.Project.FindContainingReferencesTo(_description)
                .Select(context.MakeRefactorable)
                .ToList();
            

            // only change the type AFTER we have made everything refactorable, otherwise the internal state of the
            // nodes is outdated
            _description.Parameters[parameterIndex].TypeHint = _newPortType;

            // This may lead to some connections being invalid
            // and we also need to update all affected nodes so they can refresh their port types. So we
            // start by walking over all the graphs and find nodes that are affected by this.
            foreach (var graph in graphs)
            {
                var affectedNodes = graph.GetAllNodes()
                    .Where(it => it is IReferToAnInvokable iReferToAnInvokable && iReferToAnInvokable.InvokableDescription == _description)
                    .Cast<IReferToAnInvokable>()
                    .ToList();
                
                foreach(var node in affectedNodes)
                {
                    var affectedConnections = new List<ScadConnection>();
                    var scadNode  = (ScadNode) node;
                    
                    // check which input port is responsible for the given parameter.
                    var inputPort = node.GetParameterInputPort(parameterIndex);
                    if (inputPort != -1)
                    {
                        affectedConnections.AddRange(graph.GetAllConnections().Where(it => it.IsTo(scadNode, inputPort)));    
                    }
                    
                    // same for the output ports
                    var outputPort = node.GetParameterOutputPort(parameterIndex);
                    if (outputPort != -1)
                    {
                        affectedConnections.AddRange(graph.GetAllConnections().Where(it => it.IsFrom(scadNode, outputPort)));
                    }
                    
                    // now instruct the node to rebuild its ports using the updated parameter type
                    node.SetupPorts(_description);
                    
                    // and rebuild the literal for the affected ports
                    if (inputPort != -1)
                    {
                        scadNode.DropInputPortLiteral(inputPort);
                        scadNode.BuildInputPortLiteral(inputPort);
                    }
                    
                    if (outputPort != -1)
                    {
                        scadNode.DropOutputPortLiteral(outputPort);
                        scadNode.BuildOutputPortLiteral(outputPort);
                    }
                    
                    
                    // now for all the connections we have saved, check if they are still valid.
                    affectedConnections
                        .Where(it => ConnectionRules.CanConnect(it).Decision == ConnectionRules.OperationRuleDecision.Veto )
                        .ToList()
                        // and remove the ones that are vetoed.
                        .ForAll(it => graph.RemoveConnection(it));
                }
            }
        }
    }
}