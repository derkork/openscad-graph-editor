using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Input;
using GodotXUnitApi;
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
            // it has a single output port
            Assert.Equal(1, cubeNode.OutputPortCount);
            // and when i open the help for it
            await cubeNode.Select();
            await MainWindow.Root.TypeKey(KeyList.F1);
            // the description in the help dialog is correct
            Assert.Equal("Renders an interesting cube.", MainWindow.HelpDialog.DescriptionLabel.Text);
            
            
        }
    }
}