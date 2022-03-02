using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor
{
    [UsedImplicitly]
    public class GraphEditor : Control
    {
        private GraphEdit _graphEdit;
        private AddDialog _addDialog;
        private Start _startNode;
        private TextEdit _textEdit;
        private FileDialog _fileDialog;

        private Vector2 _lastReleasePosition;
        private ScadNode _lastSourceNode;
        private ScadNode _lastDestinationNode;
        private int _lastPort;

        private string _currentFile;

        private readonly HashSet<ScadNode> _selection = new HashSet<ScadNode>();
        private bool _dirty;
        private Button _forceSaveButton;
        private Label _fileNameLabel;

        public override void _Ready()
        {
            OS.LowProcessorUsageMode = true;
            
            _graphEdit = this.WithName<GraphEdit>("GraphEdit");

            // allow to connect "Any" nodes to anything else, except "Flow" nodes.
            Enum.GetValues(typeof(PortType))
                .Cast<int>()
                .Where(x => x != (int) PortType.Flow && x != (int) PortType.Any)
                .ForAll(x =>
                {
                    _graphEdit.AddValidConnectionType((int) PortType.Any, x);
                    _graphEdit.AddValidConnectionType(x, (int) PortType.Any);
                });


            _addDialog = this.WithName<AddDialog>("AddDialog");
            _startNode = this.WithName<Start>("Start");
            _textEdit = this.WithName<TextEdit>("TextEdit");
            _fileDialog = this.WithName<FileDialog>("FileDialog");
            _fileNameLabel = this.WithName<Label>("FileNameLabel");
            _forceSaveButton = this.WithName<Button>("ForceSaveButton");
            _forceSaveButton.Connect("pressed")
                .To(this, nameof(SaveChanges));

            _graphEdit.Connect("connection_request")
                .To(this, nameof(OnConnectionRequest));
            _graphEdit.Connect("disconnection_request")
                .To(this, nameof(OnDisconnectionRequest));
            _graphEdit.Connect("connection_to_empty")
                .To(this, nameof(OnConnectionToEmpty));
            _graphEdit.Connect("connection_from_empty")
                .To(this, nameof(OnConnectionFromEmpty));
            _graphEdit.Connect("node_selected")
                .To(this, nameof(OnNodeSelected));
            _graphEdit.Connect("node_unselected")
                .To(this, nameof(OnNodeUnselected));
            _graphEdit.Connect("delete_nodes_request")
                .To(this, nameof(OnDeleteSelection));
            _graphEdit.Connect("popup_request")
                .To(this, nameof(ShowAddDialog));

            _addDialog.Connect(nameof(AddDialog.NodeSelected))
                .To(this, nameof(OnNodeAdded));

            this.WithName<Button>("PreviewButton")
                .Connect("toggled")
                .To(this, nameof(OnPreviewToggled));

            this.WithName<Button>("OpenButton")
                .Connect("pressed")
                .To(this, nameof(OnOpenFilePressed));
            
            this.WithName<Button>("SaveAsButton")
                .Connect("pressed")
                .To(this, nameof(OnSaveAsPressed));
            
            
            MarkDirty();

            this.WithName<Timer>("Timer")
                .Connect("timeout")
                .To(this, nameof(SaveChanges));
        }

        private ScadNode Named(string name)
        {
            return _graphEdit.WithName<ScadNode>(name);
        }
        
        private void OnOpenFilePressed()
        {
            _fileDialog.Mode = FileDialog.ModeEnum.OpenFile;
            _fileDialog.PopupCentered();
            _fileDialog.Connect("file_selected")
                .WithFlags(ConnectFlags.Oneshot)
                .To(this, nameof(OnOpenFile));
        }

        private void OnOpenFile(string filename)
        {
            var file = new File();
            if (!file.FileExists(filename))
            {
                return;
            }

            var savedGraph = ResourceLoader.Load<SavedGraph>(filename);
            if (savedGraph == null)
            {
                GD.Print("Cannot load file!");
                return;
            }

            var context = new ScadContext(_graphEdit);
            context.Load(savedGraph);

            _startNode = _graphEdit.GetChildNodes<Start>().First();
            
            _graphEdit.GetChildNodes<ScadNode>()
                .ForAll(it => it.ConnectChanged().To(this, nameof(MarkDirty)));
            
            _currentFile = filename;
            _fileNameLabel.Text = _currentFile;
            
            RenderScadOutput();
        }
        
        private void OnSaveAsPressed()
        {
            _fileDialog.Mode = FileDialog.ModeEnum.SaveFile;
            _fileDialog.PopupCentered();
            _fileDialog.Connect("file_selected")
                .WithFlags(ConnectFlags.Oneshot)
                .To(this, nameof(OnSaveFileSelected));
        }

        private void OnSaveFileSelected(string filename)
        {
            _currentFile = filename;
            _fileNameLabel.Text = filename;
            MarkDirty();
        }
        
        private void ShowAddDialog(Vector2 position)
        {
            if (_addDialog.Visible)
            {
                return;
            }

            _lastReleasePosition = position;

            _addDialog.Open();
        }

        private void OnPreviewToggled(bool preview)
        {
            _graphEdit.Visible = !preview;
            _textEdit.Visible = preview;
        }
        
        private void OnConnectionRequest(string from, int fromSlot, string to, int toSlot)
        {
            DoConnect(Named(from), fromSlot, Named(to), toSlot);
            MarkDirty();
        }

        private void DoConnect(ScadNode fromNode, int formPort, ScadNode toNode, int toPort)
        {
            if (fromNode == toNode)
            {
                return; // cannot connect a node to itself.
            }

            // if the source node is not an expression node then delete all connections
            // from the source port
            if (!(fromNode is ScadExpressionNode))
            {
                _graphEdit.GetConnections()
                    .Where(it => it.IsFrom(fromNode, formPort))
                    .ForAll(DoDisconnect);
            }

            // also delete all connections to the target port
            _graphEdit.GetConnections()
                .Where(it => it.IsTo(toNode, toPort))
                .ForAll(DoDisconnect);


            _graphEdit.ConnectNode(fromNode, formPort, toNode, toPort);
            fromNode.PortConnected(formPort, false);
            toNode.PortConnected(toPort, true);
        }

        private void DoDisconnect(GraphEditExt.GraphConnection connection)
        {
            connection.Disconnect();

            // notify nodes
            connection.GetFromNode<ScadNode>().PortDisconnected(connection.FromPort, false);
            connection.GetToNode<ScadNode>().PortDisconnected(connection.ToPort, true);
        }

        private void MarkDirty()
        {
            _dirty = true;
            _forceSaveButton.Text = "[...]";
        }

        private void SaveChanges()
        {
            if (!_dirty)
            {
                return;
            }

            RenderScadOutput();

            if (_currentFile != null)
            {
                // save resource
                var context = new ScadContext(_graphEdit);
                var savedGraph = context.Save();
                if (ResourceSaver.Save(_currentFile, savedGraph) != Error.Ok)
                {
                    GD.Print("Cannot save graph!");
                }

            }

            _forceSaveButton.Text = "[OK]";
            _dirty = false;

        }

        private void RenderScadOutput()
        {
            var rendered = _startNode.Render(new ScadContext(_graphEdit));
            _textEdit.Text = rendered;

            if (_currentFile != null)
            {
                // save rendered output
                var file = new File();
                if (file.Open(_currentFile + ".scad", File.ModeFlags.Write) == Error.Ok)
                {
                    file.StoreString(rendered);
                    file.Close();
                }
                else
                {
                    GD.Print("Cannot save SCAD!");
                }
            }
        }

        private void OnDisconnectionRequest(string from, int fromSlot, string to, int toSlot)
        {
            DoDisconnect(new GraphEditExt.GraphConnection(_graphEdit, from, fromSlot, to, toSlot));
            MarkDirty();
        }


        private void OnConnectionToEmpty(string from, int fromPort, Vector2 releasePosition)
        {
            _lastSourceNode = Named(from);
            _lastPort = fromPort;
            _lastReleasePosition = releasePosition;

            _addDialog.Open(it => it.HasInputThatCanConnect(_lastSourceNode.GetOutputPortType(fromPort)));
        }

        private void OnConnectionFromEmpty(string to, int toPort, Vector2 releasePosition)
        {
            _lastDestinationNode = Named(to);
            _lastPort = toPort;
            _lastReleasePosition = releasePosition;
            _addDialog.Open(it => it.HasOutputThatCanConnect(_lastDestinationNode.GetInputPortType(toPort)));
        }


        private void OnNodeAdded(ScadNode node)
        {
            node.Name = Guid.NewGuid().ToString();
            node.MoveToNewParent(_graphEdit);
            node.ConnectChanged()
                .To(this, nameof(MarkDirty));
            node.Offset = _lastReleasePosition + _graphEdit.ScrollOffset;

            if (_lastDestinationNode != null)
            {
                var index = node.GetFirstOutputThatCanConnect(_lastDestinationNode.GetInputPortType(_lastPort));
                if (index > -1)
                {
                    DoConnect(node, index, _lastDestinationNode, _lastPort);
                }
            }

            if (_lastSourceNode != null)
            {
                var index = node.GetFirstInputThatCanConnect(_lastSourceNode.GetOutputPortType(_lastPort));
                if (index > -1)
                {
                    DoConnect(_lastSourceNode, _lastPort, node, index);
                }
            }

            _lastSourceNode = null;
            _lastDestinationNode = null;
            MarkDirty();
        }

        private void OnNodeSelected(ScadNode node)
        {
            _selection.Add(node);
        }

        private void OnNodeUnselected(ScadNode node)
        {
            _selection.Remove(node);
        }

        private void OnDeleteSelection()
        {
            foreach (var node in _selection)
            {
                if (node == _startNode)
                {
                    continue; // don't allow to delete the start node
                }

                // disconnect all connections which involve the given node.
                _graphEdit.GetConnections()
                    .Where(it => it.InvolvesNode(node))
                    .ForAll(DoDisconnect);
                node.RemoveAndFree();
            }

            _selection.Clear();
            MarkDirty();
        }
    }
}