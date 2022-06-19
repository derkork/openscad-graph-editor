using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotXUnitApi;
using OpenScadGraphEditor.Tests.Drivers;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    /// <summary>
    /// Test for various types of connections.
    /// </summary>
    public class ConnectionTest : MainWindowTest
    {
        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task SimpleConnectionCanBeCreated()
        {
            // setup
            // make a cube
            await MainWindow.AddNode("cube");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            // move it a bit to the right
            await cube.DragByOwnSize(new Vector2(2, 0));


            // make a construct vector 3
            await MainWindow.AddNode("Construct Vector3");
            var constructVector3 = MainWindow.GraphEditor.Nodes.Last();
            // drag it to the left
            await constructVector3.DragByOwnSize(new Vector2(-2, 0));


            // when
            // i drag a connection from the construct vector 3 output port to the cube's first input port
            await constructVector3.DragConnection(Port.Output(0), cube, Port.Input(0));

            // then
            // the connection should be created
            Assert.True(MainWindow.GraphEditor.HasConnection(constructVector3, Port.Output(0), cube, Port.Input(0)),
                "Connection was not created.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task SimpleConnectionCanBeCreatedReverse()
        {
            // setup
            // make a cube
            await MainWindow.AddNode("cube");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            // move it a bit to the right
            await cube.DragByOwnSize(new Vector2(2, 0));


            // make a construct vector 3
            await MainWindow.AddNode("Construct Vector3");
            var constructVector3 = MainWindow.GraphEditor.Nodes.Last();
            // drag it to the left
            await constructVector3.DragByOwnSize(new Vector2(-2, 0));

            // when
            // i drag a connection in reverse from the cube's first input port to the construct vector 3's first output port
            await cube.DragConnection(Port.Input(0), constructVector3, Port.Output(0));

            // then
            // the connection should be created
            Assert.True(MainWindow.GraphEditor.HasConnection(constructVector3, Port.Output(0), cube, Port.Input(0)),
                "Connection was not created.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task SimpleConnectionCanBeRemoved()
        {
            // setup
            // make a cube and a construct vector 3, put the cube to the right of the construct vector 3
            // then connect them
            await MainWindow.AddNode("cube");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            await cube.DragByOwnSize(new Vector2(2, 0));

            await MainWindow.AddNode("Construct Vector3");
            var constructVector3 = MainWindow.GraphEditor.Nodes.Last();
            await constructVector3.DragByOwnSize(new Vector2(-2, 0));

            await constructVector3.DragConnection(Port.Output(0), cube, Port.Input(0));

            // when
            // i drag from the cube's first input port towards the construct vector 3's output port
            await cube.DragConnection(Port.Input(0), constructVector3, Port.Output(0));

            // then
            // the connection should be rekmoved
            Assert.False(
                MainWindow.GraphEditor.HasConnection(constructVector3, Port.Output(0), cube, Port.Input(0)),
                "Connection was not removed.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task ConnectionsCannotBeCreatedIfPortsMismatch()
        {
            // setup
            // make a cube
            await MainWindow.AddNode("cube");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            // move it a bit to the right
            await cube.DragByOwnSize(2, 0);

            // make another cube and move it a bit to the left
            await MainWindow.AddNode("cube");
            var cube2 = MainWindow.GraphEditor.Nodes.Last();
            await cube2.DragByOwnSize(-2, 0);

            // when
            // i drag a connection from second cube's output port to the first cube's position input port
            await cube2.DragConnection(Port.Output(0), cube, Port.Input(0));

            // then
            await DuringSeconds(1, () =>
            {
                Assert.False(MainWindow.GraphEditor.HasConnection(cube2, Port.Output(0), cube, Port.Input(0)),
                    "Connection was created.");
            });
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task PortTypeAnyCanBeConnectedToAnything()
        {
            // setup
            // make a plus node and move it a little to the left
            await MainWindow.AddNode("+");
            var plus = MainWindow.GraphEditor.Nodes.Last();
            await plus.DragByOwnSize(-2, 0);

            // make a cube and move it a little to the right
            await MainWindow.AddNode("cube");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            await cube.DragByOwnSize(2, 0);

            // when
            // i drag a connection form the plus' output port to the cube's first input port
            await plus.DragConnection(Port.Output(0), cube, Port.Input(0));

            // then
            // the connection is created
            Assert.True(MainWindow.GraphEditor.HasConnection(plus, Port.Output(0), cube, Port.Input(0)),
                "Connection was not created.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task ExpressionConnectionsReplaceOtherExpressionConnections()
        {
            // setup
            // make a plus node and move it a little up and to the left
            await MainWindow.AddNode("+");
            var plus = MainWindow.GraphEditor.Nodes.Last();
            await plus.DragByOwnSize(-2, -2);

            // make another plus node and move it a little down and to the left
            await MainWindow.AddNode("+");
            var plus2 = MainWindow.GraphEditor.Nodes.Last();
            await plus2.DragByOwnSize(-2, 2);

            // make a third plus node and move it a little to the right
            await MainWindow.AddNode("+");
            var plus3 = MainWindow.GraphEditor.Nodes.Last();
            await plus3.DragByOwnSize(2, 0);

            // when
            // now connect the first plus nodes output to the third plus node's input
            await plus.DragConnection(Port.Output(0), plus3, Port.Input(0));

            // and then connect the second plus node's output to the third plus node's input
            await plus2.DragConnection(Port.Output(0), plus3, Port.Input(0));

            // then
            // the first connection I created should be replaced by the second
            Assert.False(MainWindow.GraphEditor.HasConnection(plus, Port.Output(0), plus3, Port.Input(0)),
                "Connection was not replaced.");
            Assert.True(MainWindow.GraphEditor.HasConnection(plus2, Port.Output(0), plus3, Port.Input(0)),
                "Connection was not created.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task GeometryPortsCanTakeMultipleConnections()
        {
            // setup
            // make a "difference" node and move it a little to the right
            await MainWindow.AddNode("Difference");
            var difference = MainWindow.GraphEditor.Nodes.Last();
            await difference.DragByOwnSize(2, 0);

            // make two cube nodes and move one left + up and the other left + down
            await MainWindow.AddNode("cube");
            var cube1 = MainWindow.GraphEditor.Nodes.Last();
            await cube1.DragByOwnSize(-2, -2);

            await MainWindow.AddNode("cube");
            var cube2 = MainWindow.GraphEditor.Nodes.Last();
            await cube2.DragByOwnSize(-2, 2);

            // when
            // i drag a connection from both cube nodes' output ports to the difference's first input port
            await cube1.DragConnection(Port.Output(0), difference, Port.Input(0));
            await cube2.DragConnection(Port.Output(0), difference, Port.Input(0));

            // then
            // both connections are created
            Assert.True(MainWindow.GraphEditor.HasConnection(cube1, Port.Output(0), difference, Port.Input(0)),
                "Connection was not created.");
            Assert.True(MainWindow.GraphEditor.HasConnection(cube2, Port.Output(0), difference, Port.Input(0)),
                "Connection was not created.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task RerouteGeometryPortsCanTakeMultipleConnections()
        {
            // setup
            // make a "Reroute" node and move it a little to the right
            await MainWindow.AddNode("Reroute");
            var reroute = MainWindow.GraphEditor.Nodes.Last();
            await reroute.DragByOwnSize(2, 0);

            // make two cube nodes and move one left + up and the other left + down
            await MainWindow.AddNode("cube");
            var cube1 = MainWindow.GraphEditor.Nodes.Last();
            await cube1.DragByOwnSize(-2, -2);

            await MainWindow.AddNode("cube");
            var cube2 = MainWindow.GraphEditor.Nodes.Last();
            await cube2.DragByOwnSize(-2, 2);

            // when
            // i drag a connection from both cube nodes' output ports to the reroute node's first input port
            await cube1.DragConnection(Port.Output(0), reroute, Port.Input(0));
            await cube2.DragConnection(Port.Output(0), reroute, Port.Input(0));

            // then
            // both connections are created
            Assert.True(MainWindow.GraphEditor.HasConnection(cube1, Port.Output(0), reroute, Port.Input(0)),
                "Connection was not created.");
            Assert.True(MainWindow.GraphEditor.HasConnection(cube2, Port.Output(0), reroute, Port.Input(0)),
                "Connection was not created.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task DraggingAnOutputToAnEmptySpotOpensTheAddDialog()
        {
            // setup
            // create a cube node
            await MainWindow.AddNode("cube");
            var cube = MainWindow.GraphEditor.Nodes.Last();

            // when
            // i drag a connection from the cube's output port to an empty spot a bit right to it
            await cube.DragConnection(Port.Output(0), new Vector2(100, 0));

            // then
            // the add dialog is opened
            Assert.True(MainWindow.AddDialog.IsVisible, "Add dialog was not opened.");
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task DraggingAnInputToAnEmptySpotOpensTheAddDialog()
        {
            // setup
            // create a cube node
            await MainWindow.AddNode("cube");
            var cube = MainWindow.GraphEditor.Nodes.Last();

            // when
            // i drag a connection from the cube's input port to an empty spot a bit left to it
            await cube.DragConnection(Port.Input(0), new Vector2(-100, 0));

            // then
            // the add dialog is opened
            Assert.True(MainWindow.AddDialog.IsVisible, "Add dialog was not opened.");
        }
    }
}