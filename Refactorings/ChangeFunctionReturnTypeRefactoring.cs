using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeFunctionReturnTypeRefactoring : Refactoring
    {
        private readonly FunctionDescription _description;
        private readonly PortType _newReturnType;

        public ChangeFunctionReturnTypeRefactoring(FunctionDescription description, PortType newReturnType)
        {
            _description = description;
            _newReturnType = newReturnType;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (_description.ReturnTypeHint == _newReturnType)
            {
                return; // Nothing to do
            }


            // first find all graphs that are affected by this and make them refactorable
            var affectedNodes = context.Project.FindAllReferencingNodes(_description)
                .ToList()
                .Select(context.MakeRefactorable)
                // ensure it is all done before we continue
                .ToList();


            // only change the type AFTER we have made everything refactorable, otherwise the internal state of the
            // nodes is outdated and all kinds of 'interesting' things happen
            _description.ReturnTypeHint = _newReturnType;

            // This may lead to some connections being invalid
            // and we also need to update all affected nodes so they can refresh their port types. So we
            // start by walking over all the graphs and find nodes that are affected by this.

            foreach (var nodeReference in affectedNodes)
            {
                var affectedConnections = new List<ScadConnection>();
                var scadNode = nodeReference.Node;
                var graph = nodeReference.Graph;
                var node = (IReferToAFunction) nodeReference.NodeAsReference;

                // check which input port is responsible for the return value
                var inputPort = node.GetReturnValueInputPort();
                if (inputPort != -1)
                {
                    affectedConnections.AddRange(graph.GetAllConnections().Where(it => it.IsTo(scadNode, inputPort)));
                }

                // same for the output ports
                var outputPort = node.GetReturnValueOutputPort();
                if (outputPort != -1)
                {
                    affectedConnections.AddRange(graph.GetAllConnections()
                        .Where(it => it.IsFrom(scadNode, outputPort)));
                }

                // now instruct the node to rebuild its ports using the updated return type
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
                    .Where(it => ConnectionRules.CanConnect(it).Decision == ConnectionRules.OperationRuleDecision.Veto)
                    .ToList()
                    // and remove the ones that are vetoed.
                    .ForAll(it => graph.RemoveConnection(it));
            }
        }
    }
}