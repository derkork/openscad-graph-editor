using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using KdTree;
using KdTree.Math;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using Serilog;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// This is our main editing interface for editing graphs of invokable things (functions/modules). It is the
    /// heavyweight alternative to <see cref="ScadGraph"/>. In contrast to <see cref="ScadGraph"/>,
    /// this graph never really changes the actual graph data but just invokes refactoring events in response to
    /// user input that will do the proper refactoring actions.
    /// </summary>
    public class ScadGraphEdit : GraphEdit
    {
        private readonly HashSet<string> _selection = new HashSet<string>();
        private readonly Dictionary<string, ScadNodeWidget> _widgets = new Dictionary<string, ScadNodeWidget>();

        /// <summary>
        /// Lookup tree for finding the closest connection to a given point.
        /// </summary>
        private KdTree<float, (ScadConnection Connection, List<Vector2> BezierPoints)> _connectionTree =
            new KdTree<float, (ScadConnection Connection, List<Vector2> BezierPoints)>(2, new FloatMath());

        private ScadConnection _pendingDisconnect;
        public ScadGraph Graph { get; private set; }

        private Control _connectionHighlightLayer;

        private List<Vector2> _connectionHighlightPoints;
        private ScadConnection _highlightedConnection;
        private ScadProject _project;
        private IEditorContext _context;
        private GraphLayout _graphLayout;


        private void SelectNodes(List<ScadNode> nodes)
        {
            var set = nodes.Select(it => it.Id).ToHashSet();
            _widgets.Values.ForAll(it => it.Selected = set.Contains(it.BoundNode.Id));
            _selection.Clear();
            set.ForAll(it => OnNodeSelected(_widgets[it]));
        }


        public override void _Ready()
        {
            RightDisconnects = true;

            // we'll be handling all connection request ourselves, so we will per se allow
            // everything to be connected to everything else.
            var allPortTypes = Enum.GetValues(typeof(PortType)).Cast<PortType>().ToList();
            foreach (var from in allPortTypes)
            {
                foreach (var to in allPortTypes)
                {
                    AddValidConnectionType((int)from, (int)to);
                }
            }

            this.Connect("connection_request")
                .To(this, nameof(OnConnectionRequest));
            this.Connect("disconnection_request")
                .To(this, nameof(OnDisconnectionRequest));
            this.Connect("connection_to_empty")
                .To(this, nameof(OnConnectionToEmpty));
            this.Connect("connection_from_empty")
                .To(this, nameof(OnConnectionFromEmpty));
            this.Connect("node_selected")
                .To(this, nameof(OnNodeSelected));
            this.Connect("node_unselected")
                .To(this, nameof(OnNodeUnselected));
            this.Connect("delete_nodes_request")
                .To(this, nameof(DeleteSelection));
            this.Connect("popup_request")
                .To(this, nameof(OnPopupRequest));

            this.Connect("_end_node_move")
                .To(this, nameof(OnEndNodeMove));

            this.Connect("draw")
                .To(this, nameof(BuildConnectionLookupTree));


            _connectionHighlightLayer = this.WithName<Control>("CLAYER");
            _connectionHighlightLayer
                .Connect("draw")
                .To(this, nameof(OnConnectionHighlightLayerDraw));

            this.Connect("scroll_offset_changed")
                .To(this, nameof(OnScrollOffsetChanged));
        }

        public override bool CanDropData(Vector2 position, object data)
        {
            return data is DragData;
        }

        public override void DropData(Vector2 position, object data)
        {
            if (!(data is DragData dragData))
            {
                return; // we cannot handle this data.
            }

            if (dragData.TryGetData(out InvokableDescription invokableDescription))
            {
                // someone dragged an invokable description here, so we'll create a new node for it.
                var node = invokableDescription switch
                {
                    FunctionDescription functionDescription =>
                        NodeFactory.Build<FunctionInvocation>(functionDescription),
                    ModuleDescription moduleDescription =>
                        NodeFactory.Build<ModuleInvocation>(moduleDescription),
                    _ => null
                };

                if (node == null)
                {
                    return; // we cannot handle this data.
                }

                // ensure we don't drag in stuff that isn't usable by the graph in question.
                if (!Graph.Description.CanUse(node))
                {
                    return;
                }

                node.Offset = LocalToGraphRelative(position);
                _context.PerformRefactoring("Add node", new AddNodeRefactoring(Graph, node));
                return;
            }

            if (dragData.TryGetData(out VariableDescription variableDescription))
            {
                // in case of a variable we can either _get_ or _set_ the variable
                // so we will need to open a popup menu to let the user choose.
                var getNode = NodeFactory.Build<GetVariable>(variableDescription);
                getNode.Offset = LocalToGraphRelative(position);
                var setNode = NodeFactory.Build<SetVariable>(variableDescription);
                setNode.Offset = LocalToGraphRelative(position);

                var actions = new List<QuickAction>
                {
                    new QuickAction($"Get {variableDescription.Name}",
                        () => _context.PerformRefactoring("Add node", new AddNodeRefactoring(Graph, getNode))),
                    new QuickAction($"Set {variableDescription.Name}",
                        () => _context.PerformRefactoring("Add node", new AddNodeRefactoring(Graph, setNode)))
                };

                _context.ShowPopupMenu(GetGlobalMousePosition(), actions);
            }
        }

        private void CreateOrGet(ScadNode node)
        {
            if (!_widgets.TryGetValue(node.Id, out var widget))
            {
                widget = node is IHaveCustomWidget iUseCustomWidget
                    ? iUseCustomWidget.InstantiateCustomWidget()
                    : Prefabs.New<ScadNodeWidget>();

                // this is technically not needed but it would seem that the graph edit gets confused if you don't set
                // the name of the widget to something unique.
                widget.Name = node.Id;
                widget.LiteralToggled += (port, enabled) =>
                    PerformRefactorings("Toggle literal", new ToggleLiteralRefactoring(Graph, node, port, enabled));
                widget.LiteralValueChanged += (port, value) =>
                    PerformRefactorings("Set literal value", new SetLiteralValueRefactoring(Graph, node, port, value));
                widget.SizeChanged += size =>
                    PerformRefactorings("Change size", new ChangeNodeSizeRefactoring(Graph, node, size));

                _widgets[node.Id] = widget;
                widget.MoveToNewParent(this);
            }

            widget.BindTo(_project, Graph, node);
        }

        private void OnEndNodeMove()
        {
            // all nodes which are currently selected have moved, so we need to send a refactoring request for them.
            IEnumerable<Refactoring> refactorings = GetSelectedNodes()
                .Select(it => new ChangeNodePositionRefactoring(Graph, it, _widgets[it.Id].Offset));
            _context.PerformRefactorings("Move node(s)", refactorings);
        }

        public void FocusNode(ScadNode node)
        {
            var widget = _widgets[node.Id];
            var middle = RectSize / 2;

            // calculate the offset we need to move to get the widget into the middle.
            ScrollOffset = widget.Offset - (middle * Zoom);
            widget.Flash();
        }


        public void Render(IEditorContext context, ScadGraph graph)
        {
            Graph = graph;
            _project = context.CurrentProject;
            _context = context;
            _graphLayout = new GraphLayout(graph, _context, GlobalToGraphRelative);

            // rebuild the nodes
            foreach (var node in Graph.GetAllNodes())
            {
                CreateOrGet(node);
            }

            // redo the connections
            ClearConnections();

            foreach (var connection in Graph.GetAllConnections())
            {
                // do not show connections to wireless reroutes by default, unless the reroute node is currently selected
                if (connection.To.IsWirelessReroute() && !_selection.Contains(connection.To.Id))
                {
                    continue;
                }

                // connection contains ScadNode ids but we need to connect widgets, so first we need to find the 
                // the widget and then connect these widgets.
                ConnectNode(_widgets[connection.From.Id].Name, connection.FromPort, _widgets[connection.To.Id].Name,
                    connection.ToPort);
            }


            // highlight any bound nodes
            HighlightBoundNodes();

            // finally destroy all the widgets that are not in the graph anymore
            // set of Ids of nodes that are in the graph
            var nodeIds = graph.GetAllNodes().Select(n => n.Id).ToHashSet();
            // set of Ids of our widgets
            var widgetIds = _widgets.Keys.ToHashSet();
            // difference between the two sets
            var idsToDestroy = widgetIds.Except(nodeIds).ToList();
            // destroy all the widgets that are not in the graph anymore
            foreach (var id in idsToDestroy)
            {
                _widgets[id].RemoveAndFree();
                _widgets.Remove(id);
                // also remove them from the selection if they were in 
                _selection.Remove(id);
            }

            BuildConnectionLookupTree();

            // prevent phantom connections from being drawn
            ClearHighlightedConnection();
        }

        private void BuildConnectionLookupTree()
        {
            _connectionTree =
                new KdTree<float, (ScadConnection Connection, List<Vector2> BezierPoints)>(2, new FloatMath());
            foreach (var connection in Graph.GetAllConnections())
            {
                var connectionPoints = GetConnectionPoints(connection);
                var bezierPoints = BakeBezierLine(connectionPoints.Start, connectionPoints.End, 1);
                foreach (var bezierPoint in bezierPoints)
                {
                    _connectionTree.Add(new[] { bezierPoint.x, bezierPoint.y }, (connection, bezierPoints));
                }
            }
        }

        /// <summary>
        /// Calculates the graph relative position for any global coordinate taking zoom and scroll offset into account.
        /// </summary>
        private Vector2 GlobalToGraphRelative(Vector2 globalPosition)
        {
            var localPosition = globalPosition - RectGlobalPosition;
            return LocalToGraphRelative(localPosition);
        }

        /// <summary>
        /// Calculates the graph relative position for any local coordinate (relative to this editor) taking zoom and scroll offset into account.
        /// </summary>
        private Vector2 LocalToGraphRelative(Vector2 localPosition)
        {
            var relativePosition = localPosition;
            relativePosition += ScrollOffset;
            relativePosition /= Zoom;
            return relativePosition;
        }


        private void OnPopupRequest(Vector2 position)
        {
            var relativePosition = GlobalToGraphRelative(position);

            if (_highlightedConnection != null)
            {
                // a right click on a connection will in every case delete the connection, so first
                // make it no longer highlighted but keep the reference.
                var toDelete = _highlightedConnection;
                _highlightedConnection = null;
                _connectionHighlightPoints = null;

                // if shift is pressed we want to break the connection and insert a reroute node
                // if additionally ctrl is pressed we want to break the connection and insert a wireless reroute node
                if (KeyMap.IsShiftPressed())
                {
                    var rerouteNode = NodeFactory.Build<RerouteNode>();
                    if (KeyMap.IsCmdOrControlPressed())
                    {
                        rerouteNode.IsWireless = true;
                    }

                    rerouteNode.Offset = relativePosition;

                    PerformRefactorings("Insert reroute node",
                        // first, delete the connection
                        new DeleteConnectionRefactoring(toDelete),
                        // then insert the reroute node and connect it to the old connection's target
                        new AddNodeRefactoring(Graph, rerouteNode, toDelete.To, PortId.Input(toDelete.ToPort)),
                        // and add a new connection from the old connection's source to the reroute node
                        new AddConnectionRefactoring(new ScadConnection(Graph, toDelete.From, toDelete.FromPort,
                            rerouteNode, 0))
                    );
                    return;
                }

                // TODO: BUG - if you have a textfield selected and then right click on a connection
                // two refactorings will run at the same time, crashing the editor

                // otherwise we want to delete the connection
                PerformRefactorings("Delete connection", new DeleteConnectionRefactoring(toDelete));
                return;
            }


            if (!TryGetNodeAtPosition(relativePosition, out var node))
            {
                // right-click in empty space yields you the add dialog
                AddNodeOrOpenDialog(relativePosition);
                return;
            }

            _context.ShowPopupMenu(RequestContext.ForNode(Graph, node, position));
        }

        private bool TryGetNodeAtPosition(Vector2 relativePosition, out ScadNode node)
        {
            var firstMatchingWidget = _widgets.Values
                .FirstOrDefault(it => new Rect2(it.Offset, it.RectSize).HasPoint(relativePosition));

            node = firstMatchingWidget?.BoundNode;
            return node != null;
        }


        public override void _GuiInput(InputEvent evt)
        {
            if (evt.IsSelectAll())
            {
                SelectNodes(Graph.GetAllNodes().ToList());
                return;
            }

            if (evt.IsCopy())
            {
                CopySelection();
                return;
            }

            if (evt.IsDuplicate())
            {
                DuplicateSelection();
            }

            if (evt.IsExtract())
            {
                ExtractSelection();
                return;
            }

            if (evt.IsCut())
            {
                CutSelection();
                return;
            }

            if (evt.IsPaste())
            {
                PasteClipboard();
                return;
            }

            if (evt.IsEditComment())
            {
                CommentSelection();
                return;
            }

            if (evt.IsShowHelp())
            {
                ShowHelpForSelection();
                return;
            }

            if (evt.IsStraighten())
            {
                StraightenSelection();
                return;
            }

            if (evt.IsAlignLeft())
            {
                AlignSelectionLeft();
                return;
            }

            if (evt.IsAlignRight())
            {
                GD.Print("Align right");
                AlignSelectionRight();
                return;
            }

            if (evt.IsAlignTop())
            {
                AlignSelectionTop();
                return;
            }

            if (evt.IsAlignBottom())
            {
                AlignSelectionBottom();
                return;
            }

            if (evt is InputEventMouseButton { Pressed: false } &&
                _pendingDisconnect != null)
            {
                Log.Debug("Resolving pending disconnect");
                PerformRefactorings("Remove connection", new DeleteConnectionRefactoring(_pendingDisconnect));
                _pendingDisconnect = null;
                return;
            }

            if (evt is InputEventMouseMotion mouseMotionEvent)
            {
                // if the mouse is over a node, don't do anything
                var mousePositionRelativeToTheGraph = GlobalToGraphRelative(mouseMotionEvent.GlobalPosition);
                if (TryGetNodeAtPosition(mousePositionRelativeToTheGraph, out _))
                {
                    ClearHighlightedConnection();
                    return;
                }

                // find the closest connection to the mouse
                // we use * zoom because GlobalToGraphRelative will give us the coordinates in graph space and 
                // we need to convert them to screen space but relative to the graph widget.
                var mousePosition = mousePositionRelativeToTheGraph * Zoom;

                var closest = _connectionTree.GetNearestNeighbours(new[] { mousePosition.x, mousePosition.y }, 1);
                if (closest.Length <= 0)
                {
                    ClearHighlightedConnection();
                    return;
                }


                // are we close enough to the connection?
                var distance = new Vector2(closest[0].Point[0], closest[0].Point[1]).DistanceTo(mousePosition);
                if (distance > 20)
                {
                    ClearHighlightedConnection();
                    return;
                }

                // no need to redraw, if the connection is the same
                if (_highlightedConnection == closest[0].Value.Connection)
                {
                    return;
                }

                _highlightedConnection = closest[0].Value.Connection;
                _connectionHighlightPoints = closest[0].Value.BezierPoints;
                _connectionHighlightLayer.Update();
            }
        }

        public void CopySelection()
        {
            _context.CopyNodesToClipboard(Graph, GetSelectedNodes());
            _context.ShowInfo("Copied selection to clipboard.");
        }

        public void DuplicateSelection()
        {
            var selectedNodes = GetSelectedNodes().ToHashSet();
            var duplicatedNodes =
                Graph.CloneSelection(_context.CurrentProject, selectedNodes, out var mappedIds);

            // make a list of all refactorings that need to be performed
            var refactorings = new List<Refactoring>();

            var pastePosition = CalculatePastePosition();
            // since we already make a clone of the nodes, we set "pasteCopy" to false here. This will also help
            // with the ID-remapping for the extra connections.
            var pasteNodesRefactoring = new PasteNodesRefactoring(Graph, duplicatedNodes, pastePosition, false);

            refactorings.Add(pasteNodesRefactoring);

            // when shift is pressed we want to also duplicate all connections that go into or out of the duplicated nodes
            // https://github.com/derkork/openscad-graph-editor/issues/62
            if (KeyMap.IsShiftPressed())
            {
                // get a list of all connections that go into or out of the duplicated nodes
                // but only to nodes which are not part of the selection.
                var connectionsToDuplicate = Graph.GetAllConnections()
                    // the connection can either be from or to the duplicated nodes but not both
                    .Where(it => selectedNodes.Contains(it.From) ^ selectedNodes.Contains(it.To))
                    .ToList();

                // now walk over the connections and create a copy of each, replacing either the from or
                // to node with the new node.
                var duplicatedConnections = connectionsToDuplicate
                    .Select(it =>
                    {
                        var from = selectedNodes.Contains(it.From)
                            ? duplicatedNodes.ById(mappedIds[it.From.Id])
                            : it.From;
                        var to = selectedNodes.Contains(it.To) ? duplicatedNodes.ById(mappedIds[it.To.Id]) : it.To;
                        return new ScadConnection(Graph, from, it.FromPort, to, it.ToPort);
                    })
                    .Select(it => new AddConnectionRefactoring(it))
                    .ToList();

                refactorings.AddRange(duplicatedConnections);
            }

            var refactoringData = _context.PerformRefactorings("Duplicate nodes", refactorings);

            // select the pasted nodes
            if (refactoringData.TryGetData(PasteNodesRefactoring.PastedNodes, out var pastedNodes))
            {
                SelectNodes(pastedNodes);
            }
        }

        public void PasteClipboard()
        {
            var clipboardContents = _context.GetNodesInClipboard();
            var pastePosition = CalculatePastePosition();
            var refactoringData = _context.PerformRefactoring(
                "Paste nodes", new PasteNodesRefactoring(Graph, clipboardContents, pastePosition));

            if (refactoringData.TryGetData(PasteNodesRefactoring.PastedNodes, out var pastedNodes))
            {
                SelectNodes(pastedNodes);
            }
        }

        public void CutSelection()
        {
            // first copy the nodes to the clipboard
            var selectedNodes = GetSelectedNodes();
            _context.CopyNodesToClipboard(Graph, selectedNodes);

            // then run refactorings to delete them
            // this will implicitly also delete the connections.
            var deleteRefactorings = selectedNodes
                .Select(it => new DeleteNodeRefactoring(Graph, it))
                .ToList();

            _context.PerformRefactorings("Cut nodes", deleteRefactorings);
        }

        private Vector2 CalculatePastePosition()
        {
            return GlobalMousePositionToGraphOffset(GetGlobalMousePosition());
        }

        public void ExtractSelection()
        {
            _context.PerformRefactoring("Extract nodes",
                new ExtractInvokableRefactoring(Graph, GetSelectedNodes().ToList()));
        }

        public void StraightenSelection()
        {
            _graphLayout.StraightenConnections(GetSelectedWidgets());
        }

        public void CommentSelection()
        {
            if (!TryGetSingleSelectedNode("Please select exactly one node to edit its comment.", out var selection))
            {
                return;
            }

            _context.EditComment(Graph, selection);
        }

        public void ShowHelpForSelection()
        {
            if (!TryGetSingleSelectedNode("Please select exactly one node to show its help.", out var selection))
            {
                return;
            }

            _context.ShowHelp(Graph, selection);
        }

        public void AlignSelectionBottom()
        {
            _graphLayout.AlignNodesBottom(GetSelectedWidgets());
        }

        public void AlignSelectionTop()
        {
            _graphLayout.AlignNodesTop(GetSelectedWidgets());
        }

        public void AlignSelectionRight()
        {
            _graphLayout.AlignNodesRight(GetSelectedWidgets());
        }

        public void AlignSelectionLeft()
        {
            _graphLayout.AlignNodesLeft(GetSelectedWidgets());
        }


        private void ClearHighlightedConnection()
        {
            if (_highlightedConnection == null)
            {
                return;
            }

            _highlightedConnection = null;
            _connectionHighlightPoints = null;
            _connectionHighlightLayer.Update();
        }

        /// <summary>
        /// Calculates a graph offset from a given global mouse position. If the mouse position is outside the graph
        /// the returned position will be at 100,100 relative to the currently visible graph area.
        /// </summary>
        private Vector2 GlobalMousePositionToGraphOffset(Vector2 globalMousePosition)
        {
            // get the offset over the mouse position
            var globalRect = GetGlobalRect();

            // default position
            var result = ScrollOffset + new Vector2(100, 100);

            if (globalRect.HasPoint(globalMousePosition))
            {
                // if the mouse is inside the graph, paste the nodes at the mouse position
                result = GlobalToGraphRelative(globalMousePosition);
            }

            return result;
        }


        private void AddNodeOrOpenDialog(Vector2 position, [CanBeNull] ScadNode otherNode = null,
            PortId otherPort = default)
        {
            // when shift is pressed this means we want to have a reroute node.
            if (KeyMap.IsShiftPressed())
            {
                var rerouteNode = NodeFactory.Build<RerouteNode>();
                // when also CTRL/CMD is down, we make the reroute node a wireless node
                if (KeyMap.IsCmdOrControlPressed())
                {
                    rerouteNode.IsWireless = true;
                }

                rerouteNode.Offset = position;
                _context.PerformRefactoring("Add reroute node",
                    new AddNodeRefactoring(Graph, rerouteNode, otherNode, otherPort));
                return;
            }

            // otherwise let the user choose a node.
            var requestContext = new RequestContext().WithNodePort(Graph, otherNode, otherPort)
                .WithPosition(position);

            _context.OpenAddDialog(requestContext);
        }


        private List<ScadNode> GetSelectedNodes()
        {
            return _selection.Select(it => _widgets[it].BoundNode).ToList();
        }

        private List<ScadNodeWidget> GetSelectedWidgets()
        {
            return _selection.Select(it => _widgets[it]).ToList();
        }

        private void OnDisconnectionRequest(string fromWidgetName, int fromSlot, string toWidgetName, int toSlot)
        {
            Log.Debug("Disconnect request");
            var connection = new ScadConnection(Graph, ScadNodeForWidgetName(fromWidgetName), fromSlot,
                ScadNodeForWidgetName(toWidgetName), toSlot);

            // the disconnect is not done until the user has released the mouse button, so in case this is called
            // while the mouse is still down, just visually disconnect, but don't do a refactoring yet.
            if (Input.IsMouseButtonPressed((int)ButtonList.Left))
            {
                DisconnectNode(fromWidgetName, fromSlot, toWidgetName, toSlot);
                _pendingDisconnect = connection;
            }
        }

        private void OnConnectionToEmpty(string fromWidgetName, int fromPort, Vector2 releasePosition)
        {
            AddNodeOrOpenDialog(LocalToGraphRelative(releasePosition), ScadNodeForWidgetName(fromWidgetName),
                PortId.Output(fromPort));
        }

        private void OnConnectionFromEmpty(string toWidgetName, int toPort, Vector2 releasePosition)
        {
            AddNodeOrOpenDialog(LocalToGraphRelative(releasePosition), ScadNodeForWidgetName(toWidgetName),
                PortId.Input(toPort));
        }

        private void OnNodeSelected(ScadNodeWidget node)
        {
            _selection.Add(node.BoundNode.Id);

            // if the node was a wireless node, then temporarily show its wireless connection
            if (node.BoundNode.IsWirelessReroute())
            {
                var relevantConnections = Graph.GetAllConnections()
                    .Where(it => it.To == node.BoundNode);

                foreach (var connection in relevantConnections)
                {
                    ConnectNode(_widgets[connection.From.Id].Name, connection.FromPort, _widgets[connection.To.Id].Name,
                        connection.ToPort);
                }
            }

            HighlightBoundNodes();
        }

        private void HighlightBoundNodes()
        {
            var boundNodes = Graph.GetAllNodes().OfType<IAmBoundToOtherNode>().ToList();

            foreach (var node in boundNodes)
            {
                var partnerNodeId = node.OtherNodeId;
                var partnerNodeWidget = _widgets[partnerNodeId];
                // if the node's partner node is not currently selected, but the node is selected then highlight the node's partner node, otherwise clear it.
                if (!_selection.Contains(partnerNodeId) && _selection.Contains(((ScadNode)node).Id))
                {
                    partnerNodeWidget.Modulate = Colors.Yellow;
                }
                else
                {
                    partnerNodeWidget.Modulate = Colors.White;
                }
            }
        }

        private void OnNodeUnselected(ScadNodeWidget node)
        {
            _selection.Remove(node.BoundNode.Id);

            // if the node was a wireless node, then hide its wireless connections
            if (node.BoundNode.IsWirelessReroute())
            {
                var relevantConnections = Graph.GetAllConnections()
                    .Where(it => it.To == node.BoundNode);
                foreach (var connection in relevantConnections)
                {
                    DisconnectNode(_widgets[connection.From.Id].Name, connection.FromPort,
                        _widgets[connection.To.Id].Name, connection.ToPort);
                }
            }

            HighlightBoundNodes();
        }

        // Godot 3.4
        public void DeleteSelection()
        {
            var refactorings =
                GetSelectedNodes()
                    .Select(it => new DeleteNodeRefactoring(Graph, it, KeyMap.IsKeepConnectionsPressed()))
                    .ToList();
            _selection.Clear();
            _context.PerformRefactorings("Delete selection", refactorings);
        }

        // Godot 3.5
        private void DeleteSelection([UsedImplicitly] Node[] _)
        {
            DeleteSelection();
        }


        public void AddNode()
        {
            AddNodeOrOpenDialog(ScrollOffset + GetGlobalRect().Size / 2);
        }

        public override void _UnhandledKeyInput(InputEventKey evt)
        {
            if (KeyMap.IsRemoveNodePressed(evt))
            {
                DeleteSelection();
            }
        }

        private void OnConnectionRequest(string fromWidgetName, int fromPort, string toWidgetName, int toPort)
        {
            var connection = new ScadConnection(Graph, ScadNodeForWidgetName(fromWidgetName), fromPort,
                ScadNodeForWidgetName(toWidgetName), toPort);
            if (_pendingDisconnect != null)
            {
                if (_pendingDisconnect.RepresentsSameAs(connection))
                {
                    Log.Debug("Re-connected pending node");
                    ConnectNode(fromWidgetName, fromPort, toWidgetName, toPort);
                    _pendingDisconnect = null;
                    return;
                }
            }


            var refactorings = new AddConnectionRefactoring(connection);
            PerformRefactorings("Create connection", refactorings);
        }


        private ScadNode ScadNodeForWidgetName(string widgetName)
        {
            return this.AtPath<ScadNodeWidget>(widgetName).BoundNode;
        }


        private void PerformRefactorings(string description, params Refactoring[] refactorings)
        {
            _context.PerformRefactorings(description, refactorings);
        }

        private (Vector2 Start, Vector2 End) GetConnectionPoints(ScadConnection connection)
        {
            var fromNode = _widgets[connection.From.Id];
            var toNode = _widgets[connection.To.Id];
            var fromPort = fromNode.GetConnectionOutputPosition(connection.FromPort) + fromNode.Offset * Zoom;
            var toPort = toNode.GetConnectionInputPosition(connection.ToPort) + toNode.Offset * Zoom;
            return (fromPort, toPort);
        }


        /// <summary>
        /// Bezier interpolation function for the connection lines. This is copied straight from the Godot source code.
        /// </summary>
        private static Vector2 BezierInterpolate(float t, Vector2 start, Vector2 control1, Vector2 control2,
            Vector2 end)
        {
            var omt = 1.0f - t;
            var omt2 = omt * omt;
            var omt3 = omt2 * omt;
            var t2 = t * t;
            var t3 = t2 * t;

            return start * omt3 + control1 * omt2 * t * 3.0f + control2 * omt * t2 * 3.0f + end * t3;
        }

        /// <summary>
        /// Segment baking function for the connection lines. This is copied straight from the Godot source code + some modifications.
        /// </summary>
        private static void BakeSegment2D(List<Vector2> points, float pBegin, float pEnd,
            Vector2 pA, Vector2 pOut, Vector2 pB, Vector2 pIn, int pDepth, int pMinDepth, int pMaxDepth,
            float pTol, float pMaxLength, ref int lines)
        {
            var mp = pBegin + (pEnd - pBegin) * 0.5f;
            var beg = BezierInterpolate(pBegin, pA, pA + pOut, pB + pIn, pB);
            var mid = BezierInterpolate(mp, pA, pA + pOut, pB + pIn, pB);
            var end = BezierInterpolate(pEnd, pA, pA + pOut, pB + pIn, pB);

            var na = (mid - beg).Normalized();
            var nb = (end - mid).Normalized();
            var dp = Mathf.Rad2Deg(Mathf.Acos(na.Dot(nb)));

            // the maxLength check is to ensure we don't get longer straight segments of lines because we want 
            // to keep the points that make up the lines evenly spaced, so our KDTree can find the closest point
            // to the mouse cursor and we don't get "holes" in the detection when the mouse is over a straight line
            // that technically only needs two points to be drawn. We insert additional points this way so
            // if the line is longer the algorithm can still detect it if the mouse cursor is in the middle of it.
            if (pDepth >= pMinDepth && (dp < pTol || pDepth >= pMaxDepth) && (beg - end).Length() < pMaxLength)
            {
                points.Add(end);
                lines++;
            }
            else
            {
                BakeSegment2D(points, pBegin, mp, pA, pOut, pB, pIn, pDepth + 1, pMinDepth, pMaxDepth, pTol, pMaxLength,
                    ref lines);
                BakeSegment2D(points, mp, pEnd, pA, pOut, pB, pIn, pDepth + 1, pMinDepth, pMaxDepth, pTol, pMaxLength,
                    ref lines);
            }
        }

        /// <summary>
        /// Builds a bezier curve from the given points and returns a list of points that make up the curve.
        /// </summary>
        private List<Vector2> BakeBezierLine(Vector2 fromPoint, Vector2 toPoint, float bezierRatio)
        {
            //cubic bezier code
            var diff = toPoint.x - fromPoint.x;
            var cpLen = GetConstant("bezier_len_pos") * bezierRatio;
            var cpNegLen = GetConstant("bezier_len_neg") * bezierRatio;

            var cpOffset = diff > 0
                ? Mathf.Min(cpLen, diff * 0.5f)
                : Mathf.Max(Mathf.Min(cpLen - diff, cpNegLen), -diff * 0.5f);

            var c1 = new Vector2(cpOffset * Zoom, 0);
            var c2 = new Vector2(-cpOffset * Zoom, 0);

            var lines = 0;

            var points = new List<Vector2> { fromPoint };
            BakeSegment2D(points, 0, 1, fromPoint, c1, toPoint, c2, 0, 3, 9, 3, 20, ref lines);
            points.Add(toPoint);

            return points;
        }

        private void OnScrollOffsetChanged([UsedImplicitly] Vector2 _)
        {
            // when the scroll offset/zoom is changed our previously highlighted connection is no longer valid
            // so we need to clear it, otherwise we'll get an artifact where the connection is still highlighted
            // in its old position and scale
            ClearHighlightedConnection();
        }

        private void OnConnectionHighlightLayerDraw()
        {
            if (_highlightedConnection == null)
            {
                return;
            }

            _connectionHighlightLayer.DrawPolyline(_connectionHighlightPoints.ToArray(), new Color(1, 1, 1, 0.5f), 5,
                true);
        }

        private bool TryGetSingleSelectedNode(string warningMessage, out ScadNode result)
        {
            var selection = GetSelectedNodes().ToList();
            if (selection.Count != 1)
            {
                result = default;
                _context.ShowInfo(warningMessage);
                return false;
            }

            result = selection.First();
            return true;
        }
    }
}