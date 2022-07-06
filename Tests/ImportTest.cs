using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using GodotTestDriver.Input;
using GodotXUnitApi;
using OpenScadGraphEditor.Nodes;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class ImportTest : MainWindowTest
    {
        private string _resFolder;

        protected override async Task Setup()
        {
            _resFolder = ProjectSettings.GlobalizePath("res://Tests/Data");
            await base.Setup();
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task ImportingAScadFileWorks()
        {
            // when 
            // i press the import button
            await MainWindow.AddExternalReferenceButton.ClickCenter();
            // and i press the file select button
            await MainWindow.ImportDialog.FileSelectButton.ClickCenter();
            // and in the file dialog i set the test data folder as current folder
            await MainWindow.ImportDialog.ImportFileDialog.ChangeDirectory(_resFolder);
            // and i select "import_test.scad"
            await MainWindow.ImportDialog.ImportFileDialog.SelectFile("import_test.scad");
            // and i press "Open" in the file dialog
            await MainWindow.ImportDialog.ImportFileDialog.OpenButton.ClickCenter();
            // and i press "Ok" in the import dialog
            await MainWindow.ImportDialog.OkButton.ClickCenter();

            // then
            // i can add the "interesting_cube" module
            await MainWindow.AddNode("interesting_cube");
            var cubeNode = MainWindow.GraphEditor.Nodes.Last();
            await cubeNode.DragByOwnSize(-2, 0);

            // and i can add the "interesting_sum" function
            await MainWindow.AddNode("interesting_sum");
            var sumNode = MainWindow.GraphEditor.Nodes.Last();
            await sumNode.DragByOwnSize(2, 0);
            
            // and the "interesting_cube" module has been properly imported
            // it has two input ports
            Assert.Equal(2, cubeNode.InputPortCount);
            // the input port type is properly inferred from the argument list/documentation
            Assert.Equal((int) PortType.Vector3, cubeNode.GetPortType(Port.Input(0)));
            Assert.Equal((int) PortType.Boolean, cubeNode.GetPortType(Port.Input(1)));
            // it has a single output port
            Assert.Equal(1, cubeNode.OutputPortCount);
            Assert.Equal((int) PortType.Geometry, cubeNode.GetPortType(Port.Output(0)));
            // and when i open the help for it
            await cubeNode.Select();
            await MainWindow.Root.TypeKey(KeyList.F1);
            // the description in the help dialog is correct
            Assert.Equal("Renders an interesting cube.", MainWindow.HelpDialog.NodeDescription);
            // the description of the first import port is correct
            Assert.Equal("the size of the cube", MainWindow.HelpDialog.GetPortDescription(Port.Input(0)));
            // the description of the second import port is correct
            Assert.Equal("whether to center the cube", MainWindow.HelpDialog.GetPortDescription(Port.Input(1)));
            
            // close the help dialog to inspect the other node
            await MainWindow.HelpDialog.CloseButton.ClickCenter();
            
            // and the "interesting_sum" function has been properly imported
            // it has two input ports
            Assert.Equal(2, sumNode.InputPortCount);
            // the input port type is properly inferred from the argument list
            Assert.Equal((int) PortType.Number, sumNode.GetPortType(Port.Input(0)));
            Assert.Equal((int) PortType.Number, sumNode.GetPortType(Port.Input(1)));
            // it has a single output port
            Assert.Equal(1, sumNode.OutputPortCount);
            // the output port type is properly inferred from the documentation
            Assert.Equal((int) PortType.Number, sumNode.GetPortType(Port.Output(0)));
            
            // and when i open the help for it
            await sumNode.Select();
            await MainWindow.Root.TypeKey(KeyList.F1);
            // the description in the help dialog is correct
            Assert.Equal("Calculates the sum of the given numbers in an interesting way.", MainWindow.HelpDialog.NodeDescription);
            // the description of the first import port is correct
            Assert.Equal("the first number to add", MainWindow.HelpDialog.GetPortDescription(Port.Input(0)));
            // the description of the second import port is correct
            Assert.Equal("the second number to add", MainWindow.HelpDialog.GetPortDescription(Port.Input(1)));
            // the description of the output port is correct
            Assert.Equal("the sum of the given numbers", MainWindow.HelpDialog.GetPortDescription(Port.Output(0)));
        }
    }
}