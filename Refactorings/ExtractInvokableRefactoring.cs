using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Refactorings
{
    public class ExtractInvokableRefactoring : Refactoring, IProvideTicketData<InvokableDescription>
    {
        private readonly ScadGraph _sourceGraph;
        private readonly List<ScadNode> _selection;

        public string Ticket { get; } = Guid.NewGuid().ToString();

        public ExtractInvokableRefactoring(ScadGraph sourceGraph, List<ScadNode> selection)
        {
            _sourceGraph = sourceGraph;
            _selection = selection;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // PHASE 1: Analysis
            // ====================================================================================================

            var clones = _sourceGraph.CloneSelection(context.Project, _selection, out var newIds);

            // now we need to check if the selection contains any nodes which are not an expression.
            // if so, we need to create a module, otherwise we need to create a function.

            var isModule = clones.GetAllNodes().Any(it => !(it is IAmAnExpression));

            // To determine parameters we need to find all connections that are connecting from outside the selection
            // to inside the selection.
            var incomingConnections = _sourceGraph.GetAllConnections()
                // we use the id mapping here as this has the sanitized selection
                .Where(it => newIds.ContainsKey(it.To.Id) && !newIds.ContainsKey(it.From.Id))
                // sort them by vertical position to avoid crossing lines later
                .OrderBy(it => it.From.Offset.y)
                .ThenBy(it => it.From.Offset.x)
                .ToList();

            // to determine return values we need to find all connections that are connecting from inside the selection
            // to outside the selection.
            var outgoingConnections = _sourceGraph.GetAllConnections()
                // we use the id mapping here as this has the sanitized selection
                .Where(it => !newIds.ContainsKey(it.To.Id) && newIds.ContainsKey(it.From.Id))
                .ToList();

            // now a few sanity checks.
            if (isModule)
            {
                // modules don't need to have any outgoing connections
                if (outgoingConnections.Count > 0)
                {
                    // if they have them, they all need to be of type "Geometry" 
                    if (outgoingConnections.Any(it => it.TryGetFromPortType(out var type) && type != PortType.Geometry))
                    {
                        NotificationService.ShowNotification(
                            "Cannot extract module. The return values must all be of type 'geometry'.");
                        return;
                    }

                    // making a module is possible under the following conditions:
                    // - all geometry nodes in the selection are connected to either a node outside the selection or
                    //   to the output of the selection (since we have outgoing connections we can ignore the case
                    //   where there are no geometry nodes at all or all geometry nodes are disconnected)

                    var geometryNodes = _selection.Where(it => !(it is IAmAnExpression)).ToList();
                    // check at at least one connection exists where the geometry node is the source
                    if (geometryNodes.Any(it => _sourceGraph.GetAllConnections().All(con => con.From != it)))
                    {
                        NotificationService.ShowNotification(
                            "Cannot extract module. The selection contains some geometry which is not connected to anything and some that is.");
                        return;
                    }

                    // now we also need to check that for all outgoing connections one of the following is true:
                    // - all outgoing connections go to the same node outside the selection
                    // - all outgoing connections originate from the same node inside the selection
                    // - all originating nodes inside the selection are connected to all outgoing nodes outside the selection

                    var sources = outgoingConnections.Select(it => it.From).Distinct().ToList();
                    var destinations = outgoingConnections.Select(it => it.To).Distinct().ToList();

                    if (sources.Count != 1 && destinations.Count != 1 &&
                        // assuming we have no duplicate connections
                        outgoingConnections.Count != sources.Count * destinations.Count)
                    {
                        NotificationService.ShowNotification(
                            "Cannot extract module. The selection contains multiple geometry outputs that would get lost when extracting the module.");
                        return;
                    }
                }
            }
            else
            {
                // for functions there must be no more than one outgoing connection
                if (outgoingConnections.Count > 1)
                {
                    NotificationService.ShowNotification(
                        $"Cannot extract function. There must be at most one return value for a function but there would be {outgoingConnections.Count}.");
                    return;
                }

                // also all incoming connections must be of expression types
                if (incomingConnections.Any(it => it.TryGetToPortType(out var type) && !type.IsExpressionType()))
                {
                    NotificationService.ShowNotification(
                        "Cannot extract function. All parameters must be expressions.");
                    return;
                }
            }

            // PHASE 2: Create the invokable and its graph
            // ====================================================================================================

            // now build an invokable description from the data we have collected
            InvokableDescription invokableDescription;
            if (isModule)
            {
                var builder = ModuleBuilder.NewModule(context.Project.SafeName("new_module"));

                // now we create all incoming parameters by using the incoming connections
                // that expression types
                var nonGeometryIncomingConnections = incomingConnections.Where(it =>
                    it.TryGetToPortType(out var type) && type.IsExpressionType()).ToList();

                BuildParameters(nonGeometryIncomingConnections)
                    .ForAll(it => builder.WithParameter(it.Name, it.Type));

                invokableDescription = builder.Build();
            }
            else
            {
                // determine the return type from the outgoing connection
                // if we have no outgoing connection, we default to "any"
                var returnType = PortType.Any;
                if (outgoingConnections.Count > 0)
                {
                    returnType = outgoingConnections.First().TryGetFromPortType(out var type) ? type : PortType.Any;
                }

                var builder =
                    FunctionBuilder.NewFunction(context.Project.SafeName("new_function"), returnType: returnType);

                // now we create all incoming parameters by using the incoming connections (there should be none of type "Geometry")
                BuildParameters(incomingConnections)
                    .ForAll(it => builder.WithParameter(it.Name, it.Type));

                invokableDescription = builder.Build();
            }


            // now create a graph for the invokable
            var newGraph = context.Project.AddInvokable(invokableDescription);

            // PHASE 3: Populate the graph
            // ====================================================================================================

            // find the entry point
            var entryPoint = newGraph.GetAllNodes().First(it => it is EntryPoint) as EntryPoint;

            // paste the selection into the new graph, right next to the entry point
            var pastePosition = entryPoint.Offset + new Vector2(200, 0);
            context.PerformRefactoring(new PasteNodesRefactoring(newGraph, clones, pastePosition, false));

            // now connect the parameters from the entry point to the nodes inside the selection
            // for expression types
            var parameterList = incomingConnections
                .Where(it => it.TryGetToPortType(out var type) && type.IsExpressionType())
                .ToList();

            // the entry point should now have output ports for each parameter in the same order as
            // the parameter list
            for (var i = 0; i < parameterList.Count; i++)
            {
                var connection = parameterList[i];
                var newConnection = new ScadConnection(newGraph, entryPoint, i, newGraph.ById(newIds[connection.To.Id]),
                    connection.ToPort);
                context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
            }

            // if we have a module, we also need to create 'child' nodes for incoming connections which are not 
            // expression types. If we have only a single source of geometry, then we'll use a 'children()' node,
            // otherwise we'll use a 'child(x)' node for each incoming connection. We also sort nodes by their
            // position in the selection, so we connect stuff in the right order (e.g. top left is rendered before
            // bottom right)
            if (isModule)
            {
                var geometryInputs = incomingConnections
                    .Where(it => it.TryGetToPortType(out var type) && type == PortType.Geometry)
                    .ToList();

                // if we don't have any geometry inputs, we can skip this
                if (geometryInputs.Count > 0)
                {
                    // enable children for this graph, because we'll need it
                    context.PerformRefactoring(new EnableChildrenRefactoring((ModuleDescription) invokableDescription));
                    
                    // check how many sources and destinations we have
                    var sources = geometryInputs.Select(it => it.From).Distinct().ToList();
                    var destinations = geometryInputs.Select(it => it.To).Distinct().ToList();

                    // we can use a single children() node if any of these three conditions is true:
                    // - we have only a single source
                    // - we have only a single destination
                    // - all sources are connected to all destinations

                    var useSingleChildrenNode = sources.Count == 1 || destinations.Count == 1 ||
                                                // assuming there are no duplicate connections
                                                sources.Count * destinations.Count == geometryInputs.Count;

                    if (useSingleChildrenNode)
                    {
                        var childrenNode = NodeFactory.Build<Children>();

                        // add the node to the graph
                        context.PerformRefactoring(new AddNodeRefactoring(newGraph, childrenNode));

                        // position it below the entry point
                        context.PerformRefactoring(new ChangeNodePositionRefactoring(newGraph, childrenNode,
                            entryPoint.Offset + new Vector2(0, 200)));

                        // and connect it's output to all destinations
                        foreach (var connection in geometryInputs)
                        {
                            var newConnection = new ScadConnection(newGraph, childrenNode, 0,
                                newGraph.ById(newIds[connection.To.Id]), connection.ToPort);
                            context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                        }
                    }
                    else
                    {
                        // it's a bit more tricky here, we need to create a child(x) node for each incoming source
                        // and connect it to the corresponding destination.
                        
                        // first create a child node for each source and store it in a dictionary
                        var childNodes = new Dictionary<ScadNode, Child>();
                        for (var i = 0; i < sources.Count; i++)
                        {
                            var sourceNode = sources[i];
                            var childNode = NodeFactory.Build<Child>();
                            childNodes.Add(sourceNode, childNode);
                            
                            // add the node to the graph
                            context.PerformRefactoring(new AddNodeRefactoring(newGraph, childNode));
                            
                            // position it below the entry point, 200 units down below the previous child
                            context.PerformRefactoring(new ChangeNodePositionRefactoring(newGraph, childNode,
                                entryPoint.Offset + new Vector2(0, 200 + i * 200)));
                            
                            // also set the input port literal of the child node to the index, so first node
                            // gets 0, second node gets 1, etc.
                            context.PerformRefactoring(new SetLiteralValueRefactoring(newGraph, childNode,
                                PortId.Input(0), new NumberLiteral(i)));
                        }

                        foreach (var connection in geometryInputs)
                        {
                            if (!childNodes.TryGetValue(connection.From, out var childNode))
                            {
                                NotificationService.ShowBug("Cannot find child node for source node.");
                                continue;
                            }

                            // and connect it's output to the corresponding destination
                            var newConnection = new ScadConnection(newGraph, childNode, 0,
                                newGraph.ById(newIds[connection.To.Id]), connection.ToPort);

                            context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                        }
                    }
                }
            }
            else
            {
                // for functions, we need to connect the function result to the return value
                var functionReturn = newGraph.GetAllNodes().First(it => it is FunctionReturn) as FunctionReturn;

                // determine the node which is most to the right
                var rightMostNode = newGraph.GetAllNodes()
                    .Where(it => it != functionReturn)
                    .OrderByDescending(it => it.Offset.x)
                    .First();

                // move the function return so it is to the right of the right-most node
                context.PerformRefactoring(new ChangeNodePositionRefactoring(newGraph, functionReturn,
                    rightMostNode.Offset + new Vector2(200, 0)));

                // now if we have an outgoing connection, find the matching new node and connect it to the function return
                if (outgoingConnections.Count == 1)
                {
                    var connection = outgoingConnections.First();
                    var newConnection = new ScadConnection(newGraph, newGraph.ById(newIds[connection.From.Id]),
                        connection.FromPort,
                        functionReturn, 0);
                    context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                }
                else
                {
                    // check if we have a single expression node which does not have its output connected. in that 
                    // case use this as the return value
                    var candidates = newGraph.GetAllNodes()
                        .Where(it => it is IAmAnExpression)
                        .ToList();

                    var allConnections = newGraph.GetAllConnections();

                    // find all nodes which are not the source of a connection
                    var unconnectedNodes = candidates
                        .Where(it => allConnections.All(c => c.From != it))
                        .ToList();

                    // if we have exactly one, use that
                    if (unconnectedNodes.Count == 1)
                    {
                        var connection = new ScadConnection(newGraph, unconnectedNodes[0], 0, functionReturn, 0);
                        context.PerformRefactoring(new AddConnectionRefactoring(connection));
                    }
                }
            }

            // Phase 4: remove the original nodes and connections and replace them with a call to the new invokable
            // ====================================================================================================
            
            // it is enough to delete the nodes, the refactoring will kill all connections automatically
            foreach (var node in _selection)
            {
                context.PerformRefactoring(new DeleteNodeRefactoring(_sourceGraph, node));
            }

            // put the new function / module roughly at the center of all the nodes we just deleted
            var center = _selection.Select(it => it.Offset).Aggregate((a, b) => a + b) / _selection.Count;

            var newInvocation = isModule
                ? NodeFactory.Build<ModuleInvocation>(invokableDescription)
                : NodeFactory.Build<FunctionInvocation>(invokableDescription);

            context.PerformRefactoring(new AddNodeRefactoring(_sourceGraph, newInvocation));
            context.PerformRefactoring(new ChangeNodePositionRefactoring(_sourceGraph, newInvocation, center));

            // now connect the parameters, we need to check however if the module supports children() or not
            // because that changes the offset of the parameters
            var hasChildren = isModule && ((ModuleDescription) invokableDescription).SupportsChildren;
            var parameterOffset = hasChildren ? 1 : 0;

            // only connect expression inputs
            var parameterConnections = incomingConnections
                .Where(it => it.TryGetFromPortType(out var type) && type.IsExpressionType())
                .ToList();

            for (var i = 0; i < parameterConnections.Count; i++)
            {
                var connection = parameterConnections[i];
                var newConnection = new ScadConnection(_sourceGraph, connection.From, connection.FromPort,
                    newInvocation,
                    i + parameterOffset);
                context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
            }

            // connect all geometry inputs to the geometry input port 
            var geometryConnections = incomingConnections
                .Where(it => it.TryGetToPortType(out var type) && type == PortType.Geometry)
                .ToList();

            foreach (var connection in geometryConnections)
            {
                var newConnection = new ScadConnection(_sourceGraph, connection.From, connection.FromPort,
                    newInvocation,
                    0); // first port is the geometry input
                context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
            }

            // connect geometry outputs
            if (isModule)
            {
                foreach (var connection in outgoingConnections)
                {
                    var newConnection =
                        new ScadConnection(_sourceGraph, newInvocation, 0, connection.To, connection.ToPort);
                    context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                }
            }
            else
            {
                // if we have a single output connection, connect the return value to the node
                if (outgoingConnections.Count == 1)
                {
                    var newConnection = new ScadConnection(_sourceGraph, newInvocation, 0, outgoingConnections[0].To,
                        outgoingConnections[0].ToPort);
                    context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                }
            }
        }

        private static List<(string Name, PortType Type)> BuildParameters(
            IEnumerable<ScadConnection> incomingConnections)
        {
            return incomingConnections
                .Select(it =>
                {
                    var portType = it.TryGetFromPortType(out var type) ? type : PortType.Any;
                    
                    // check the source node where the connection comes from
                    var sourceNode = it.From;
                    
                    // if the source node is a variable getter, use the variable name as the parameter name
                    if (sourceNode is GetVariable variableGetter)
                    {
                        return (variableGetter.VariableDescription.Name, portType);
                    }
                    
                    // if the source node is an entry point, use the parameter name that matches the port
                    if (sourceNode is EntryPoint entryPoint)
                    {
                        // if the entry point refers to an invokable, get it's description
                        if (entryPoint is IReferToAnInvokable referToAnInvokable)
                        {
                            var invokableDescription = referToAnInvokable.InvokableDescription;
                            if (invokableDescription.Parameters.Count > it.FromPort)
                            {
                                return (invokableDescription.Parameters[it.FromPort].Name, portType);
                            }
                        }
                    }
                    
                    // if the source node otherwise refers to an invokable, use the name of the invokable as the parameter name
                    if (sourceNode is IReferToAnInvokable referToAnInvokable2)
                    {
                        return (referToAnInvokable2.InvokableDescription.Name, portType);
                    }
                    
                    // if there is a reasonable short comment on the source node use that
                    if (sourceNode.TryGetComment(out var comment) && comment.Length < 20)
                    {
                        return (comment.AsSafeIdentifier(), portType);
                    }
                    
                    // well, no more clever ideas, so just use "parameter"
                    return ("parameter", portType);
                }).ToList();
        }
    }
}