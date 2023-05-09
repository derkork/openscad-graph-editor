﻿using System;
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
            
            var clones = _sourceGraph.CloneSelection(context.Project, _selection, out var newIds);

            // now we need to check if the selection contains any nodes which are not an expression.
            // if so, we need to create a module, otherwise we need to create a function.

            var isModule = clones.GetAllNodes().Any(it => !(it is IAmAnExpression));

            // To determine parameters we need to find all connections that are connecting from outside the selection
            // to inside the selection.
            var incomingConnections = _sourceGraph.GetAllConnections()
                // we use the id mapping here as this has the sanitized selection
                .Where(it => newIds.ContainsKey(it.To.Id) && !newIds.ContainsKey(it.From.Id))
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
                        NotificationService.ShowNotification("Cannot extract module. The selection contains some geometry which is not connected to anything and some that is.");
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
                        NotificationService.ShowNotification("Cannot extract module. The selection contains multiple geometry outputs that would get lost when extracting the module.");
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
                        // it's a bit more tricky here, we need to create a child(x) node for each incoming connection
                        // and connect it to the corresponding destination.
                        // first sort the connection by the position of the source node, so we know which child is
                        // first and last.
                        var sortedGeometryInputs = geometryInputs
                            .OrderBy(it => it.From.Offset.y)
                            .ThenBy(it => it.From.Offset.x)
                            .ToList();

                        for (var index = 0; index < sortedGeometryInputs.Count; index++)
                        {
                            var connection = sortedGeometryInputs[index];
                            
                            // make a new child node
                            var childNode = NodeFactory.Build<Child>();

                            // add the node to the graph
                            context.PerformRefactoring(new AddNodeRefactoring(newGraph, childNode));
                            // position it below the entry point, 200 units down below the previous child
                            context.PerformRefactoring(new ChangeNodePositionRefactoring(newGraph, childNode, 
                                entryPoint.Offset + new Vector2(0, 200 + index * 200)));
                            
                            // and connect it's output to the corresponding destination
                            var newConnection = new ScadConnection(newGraph, childNode, 0, 
                                newGraph.ById(newIds[connection.To.Id]), connection.ToPort);
                            
                            context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                            
                            // also set the input port literal of the child node to the index, so first node
                            // gets 0, second node gets 1, etc.
                            context.PerformRefactoring(new SetLiteralValueRefactoring(newGraph, childNode, PortId.Input(0), new NumberLiteral(index)));
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
                    var newConnection = new ScadConnection(newGraph, newGraph.ById(newIds[connection.From.Id]), connection.FromPort, 
                        functionReturn, 0);
                    context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                }
                // TODO: find if we have exactly one unconnected expression output port and connect it to the function return
            }
        }

        private static List<(string Name, PortType Type)> BuildParameters(IEnumerable<ScadConnection> incomingConnections)
        {
            return incomingConnections
                .Select(it =>
                {
                    var portType = it.TryGetFromPortType(out var type) ? type : PortType.Any;
                    // TODO: get a bit cleverer here and try to deduce a good name
                    return ("parameter", portType);
                }).ToList();
        }
    }
}