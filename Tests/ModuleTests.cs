using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
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
            Assert.Equal("test_module", node.NodeTitle);
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
            // and i select the type to be "boolean"
            await parameterLine.Type.SelectItemWithText("boolean");
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();

            // then
            // a single node is shown in the graph editor
            Assert.Single(MainWindow.GraphEditor.Nodes);
            var node = MainWindow.GraphEditor.Nodes.First();
            // and the node has the correct title
            Assert.Equal("test_module", node.NodeTitle);
            // and the node has no input ports
            Assert.Equal(0, node.InputPortCount);
            // and the node has one output port
            Assert.Equal(1, node.OutputPortCount);
            // and the node has a toggle button to enable a literal for the output port
            Assert.True(node.ToggleButton(Port.Output(0)).IsVisible);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task RenamingAModuleWorks()
        {
            // when
            // i add a module
            await MainWindow.AddModuleButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();

            // and i create a call to this module in main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            await MainWindow.AddNode("test_module");

            // and i then go back to the module editor
            await MainWindow.TabContainer.SelectTabWithTitle("test_module");

            // and i right-click the module entrypoint
            await MainWindow.GraphEditor.Nodes.First().ClickAtSelectionSpot(ButtonList.Right);
            // and i select the "Refactor test_module" menu item
            await MainWindow.PopupMenu.SelectItemWithText("Refactor test_module");
            // and i enter a new name in the invokable refactoring dialog
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("renamed_module");
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();

            // then
            // the module is renamed
            Assert.Equal("renamed_module", MainWindow.GraphEditor.Nodes.First().NodeTitle);
            // the tab title is also renamed
            Assert.Equal("renamed_module", MainWindow.TabContainer.SelectedTabTitle);
            // and when i get back to the main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            // and the call to the module is renamed as well
            Assert.Equal("renamed_module", MainWindow.GraphEditor.Nodes.First().NodeTitle);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task MakingAParameterOptionalWorks()
        {
            // when
            // i add a module
            await MainWindow.AddModuleButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            // and i add a parameter
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            var parameterLine = MainWindow.InvokableRefactorDialog.ParameterLines.First();
            // and i set the parameter name to "foo"
            await parameterLine.NameEdit.Type("foo");
            // and i select the type to be "boolean"
            await parameterLine.Type.SelectItemWithText("boolean");
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();


            // and i create a call to this module in main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            await MainWindow.AddNode("test_module");
            var moduleInvocation = MainWindow.GraphEditor.Nodes.First();

            // then 
            // the module has no toggle button for the parameter, because it is mandatory
            Assert.False(moduleInvocation.ToggleButton(Port.Input(0)).IsVisible);


            // and i then go back to the module editor
            await MainWindow.TabContainer.SelectTabWithTitle("test_module");
            // and i click the toggle button on the entrypoint parameter
            // so it is now optional
            var entrypointNode = MainWindow.GraphEditor.Nodes.First();
            await entrypointNode.ToggleButton(Port.Output(0)).ClickCenter();

            // then
            // the entrypoint now shows a checkbox for the parameter default value
            Assert.True(entrypointNode.CheckBoxLiteral(Port.Output(0)).IsVisible);

            // and when i get back to the main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            // the module invocation now has a toggle button for setting the parameter value
            Assert.True(moduleInvocation.ToggleButton(Port.Input(0)).IsVisible);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task ReorderingParametersWorks()
        {
            // when
            // i add a module
            await MainWindow.AddModuleButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            // and i add two parameters
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            var lines = MainWindow.InvokableRefactorDialog.ParameterLines.ToList();
            var firstLine = lines[0];
            var secondLine = lines[1];

            // and i set the first parameter name to "foo"
            await firstLine.NameEdit.Type("foo");
            // and i select the type to be "vector3"
            await firstLine.Type.SelectItemWithText("vector3");
            // and i set the second parameter name to "bar"
            await secondLine.NameEdit.Type("bar");
            // and i select the type to be "boolean"
            await secondLine.Type.SelectItemWithText("boolean");
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            var moduleEntryPoint = MainWindow.GraphEditor.Nodes.First();

            // and i add a cube node
            await MainWindow.AddNode("cube");
            var cubeNode = MainWindow.GraphEditor.Nodes.Last();

            // and i connect the first output of the module entrypoint to the first input of the cube node
            await moduleEntryPoint.DragConnection(Port.Output(0), cubeNode, Port.Input(0));
            // and i connect the second output of the module entrypoint to the second input of the cube node
            await moduleEntryPoint.DragConnection(Port.Output(1), cubeNode, Port.Input(1));

            // and i create a call to this module in main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            await MainWindow.AddNode("test_module");
            var moduleInvocation = MainWindow.GraphEditor.Nodes.First();

            // and i move this a bit to the right
            await moduleInvocation.DragByOwnSize(2, 0);

            // and i add a construct vector3
            await MainWindow.AddNode("Construct Vector3");
            var constructVector3 = MainWindow.GraphEditor.Nodes.Last();
            // and i move it a bit to the left and up
            await constructVector3.DragByOwnSize(-2, -1);

            // and i add a boolean "AND"
            await MainWindow.AddNode("Boolean And");
            var booleanAnd = MainWindow.GraphEditor.Nodes.Last();
            // and i move it a bit to the left and down
            await booleanAnd.DragByOwnSize(-2, 1);

            // and i connect the output of the construct vector3 to the first input of the module invocation
            await constructVector3.DragConnection(Port.Output(0), moduleInvocation, Port.Input(0));
            // and i connect the output of the boolean and to the second input of the module invocation
            await booleanAnd.DragConnection(Port.Output(0), moduleInvocation, Port.Input(1));

            // and i go back to the module editor
            await MainWindow.TabContainer.SelectTabWithTitle("test_module");
            // and i right-click the module entry point
            await moduleEntryPoint.ClickAtSelectionSpot(ButtonList.Right);
            // and i select the "Refactor test_module" menu item
            await MainWindow.PopupMenu.SelectItemWithText("Refactor test_module");

            // and i click the "move up" button in the second parameter row
            await MainWindow.InvokableRefactorDialog.ParameterLines.Skip(1).First().UpButton.ClickCenter();
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();

            // then
            // the name of the first parameter is now "bar"
            Assert.Equal("bar", moduleEntryPoint.PortLabel(Port.Output(0)).Text);
            // the name of the second parameter is now "foo"
            Assert.Equal("foo", moduleEntryPoint.PortLabel(Port.Output(1)).Text);
            // the first output of the module entry point is now connected to the second input of the cube node
            Assert.True(
                MainWindow.GraphEditor.HasConnection(moduleEntryPoint, Port.Output(0), cubeNode, Port.Input(1))
            );
            // the second output of the module entry point is now connected to the first input of the cube node
            Assert.True(
                MainWindow.GraphEditor.HasConnection(moduleEntryPoint, Port.Output(1), cubeNode, Port.Input(0))
            );
            
            // and when i go back to the main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            
            // the output of the "construct vector3" is now connected to the second input of the module invocation
            Assert.True(
                MainWindow.GraphEditor.HasConnection(constructVector3, Port.Output(0), moduleInvocation, Port.Input(1))
            );
            // the output of the "boolean and" is now connected to the first input of the module invocation
            Assert.True(
                MainWindow.GraphEditor.HasConnection(booleanAnd, Port.Output(0), moduleInvocation, Port.Input(0))
            );
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task AddingAParameterWorks()
        {
            // when
            // i add a module
            await MainWindow.AddModuleButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            var moduleEntryPoint = MainWindow.GraphEditor.Nodes.First();
            
            // and i add an instance of this to the main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            await MainWindow.AddNode("test_module");
            var moduleInvocation = MainWindow.GraphEditor.Nodes.First();
            
            // and i go back to the module editor
            await MainWindow.TabContainer.SelectTabWithTitle("test_module");
            // and i right-click the module entry point
            await moduleEntryPoint.ClickAtSelectionSpot(ButtonList.Right);
            // and i select the "Refactor test_module" menu item
            await MainWindow.PopupMenu.SelectItemWithText("Refactor test_module");
            
            // and i add a parameter
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            var line = MainWindow.InvokableRefactorDialog.ParameterLines.First();
            await line.Type.SelectItemWithText("boolean");
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            
            // then
            // the module entry point has a new parameter
            Assert.Equal(1, moduleEntryPoint.OutputPortCount);
            // and there is a toggle button at the new parameter
            Assert.True(moduleEntryPoint.ToggleButton(Port.Output(0)).IsVisible);
            
            // and when i go back to the main module
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            // the module invocation has a new parameter
            Assert.Equal(1, moduleInvocation.InputPortCount);
            // and there is a checkbox where the new parameter value can be set
            Assert.True(moduleInvocation.CheckBoxLiteral(Port.Input(0)).IsVisible);
        }
    }
}