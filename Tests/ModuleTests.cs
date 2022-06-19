using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotExt;
using GodotXUnitApi;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class ModuleTests : MainWindowTest
    {
        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task AddingAModuleWorks()
        {
            // when 
            // press the add module button
            await MainWindow.AddModuleButton.ClickCenter();
            // and enter a module name in the invokable refactoring dialog
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            // and press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            
            // then
            // we have a tab with the module and this tab is selected
            Assert.Equal("test_module", MainWindow.TabContainer.SelectedTabTitle);
            
            // a single node is shown in the graph editor
            Assert.Single(MainWindow.GraphEditor.Nodes);
            var node = MainWindow.GraphEditor.Nodes.First();
            
            // and the node has the correct title
            Assert.Equal("test_module", node.Title);
            // and the node has no output ports
            Assert.Equal(0, node.OutputPortCount);
            // and the node has no input ports
            Assert.Equal(0, node.InputPortCount);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task AddingAModuleWithParametersWorks()
        {
            // when
            // press the add module button
            await MainWindow.AddModuleButton.ClickCenter();
            // and enter a module name in the invokable refactoring dialog
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            // and i add a parameter
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            var parameterLine = MainWindow.InvokableRefactorDialog.ParameterLines.First();
            // and i set the parameter name to "foo"
            await parameterLine.NameEdit.Type("foo");
            // and i select the type to be "number"
            await parameterLine.Type.SelectItemWithText("number");
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            
            // then
            // a single node is shown in the graph editor
            Assert.Single(MainWindow.GraphEditor.Nodes);
            var node = MainWindow.GraphEditor.Nodes.First();
            // and the node has the correct title
            Assert.Equal("test_module", node.Title);
            // and the node has one output port
            Assert.Equal(1, node.OutputPortCount);
            // and the node has no input ports
            Assert.Equal(0, node.InputPortCount);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task RenamingAModuleWorks()
        {
            // when
            // i add a module
            await MainWindow.AddModuleButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();

            // and i change its name afterwards
        }
    }
}