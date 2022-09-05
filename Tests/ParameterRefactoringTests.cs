using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using GodotXUnitApi;
using OpenScadGraphEditor.Nodes;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class ParameterRefactoringTests : MainWindowTest
    {
        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task ChangingParameterTypesKeepsConnections()
        {
            // when i create a module
            var moduleName = "mod" + Guid.NewGuid().ToString().Replace("-", "");
            await MainWindow.AddModuleButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.NameEdit.Type(moduleName);
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            
            var entryPoint = MainWindow.GraphEditor.Nodes.Last();

            // and i add a plus node to the module
            await MainWindow.AddNode("++");
            var plusNodeModule = MainWindow.GraphEditor.Nodes.Last();
            
            // and i connect the first output of the entry point to the first input of the plus node
            await entryPoint.DragConnection(Port.Output(0), plusNodeModule, Port.Input(0));
            
            // and i switch to the main graph
            await MainWindow.TabContainer.SelectTabWithTitle("<main>");
            
            // and i create a module instance
            await MainWindow.AddNode(moduleName);
            var invocation = MainWindow.GraphEditor.Nodes.Last();
            
            // and i move this a bit to the right
            await invocation.DragByOwnSize(2, 0);
            
            // and i create an add node
            await MainWindow.AddNode("++");
            var plusNodeMain = MainWindow.GraphEditor.Nodes.Last();
            // and i drag this a bit to the left
            await plusNodeMain.DragByOwnSize(-2, 0);
            
            // and i connect the output of the plus node to the input of the invocation
            await plusNodeMain.DragConnection(Port.Output(0), invocation, Port.Input(0));
            
            // and i right-click the module invocation
            await invocation.ClickAtSelectionSpot(ButtonList.Right);
            // and i select  the "Refactor <module>" entry in the popup menu
            await MainWindow.PopupMenu.SelectItemWithText("Refactor " + moduleName);
            
            // and i change the type of the parameter to "number"
            await MainWindow.InvokableRefactorDialog.ParameterLines.First().Type.SelectItemWithText("number");
            // and i press the "OK" button
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            
            // then
            // the input type of the invocation is now "number"
            Assert.Equal((int)PortType.Number, invocation.GetPortType(Port.Input(0)));
            
            // and the plus node in the main graph is still connected to the invocation
            Assert.True(MainWindow.GraphEditor.HasConnection(plusNodeMain, Port.Output(0), invocation, Port.Input(0)));
            
            // and when i go back to the module
            await MainWindow.TabContainer.SelectTabWithTitle(moduleName);
            
            // then the plus node in the module is still connected to the entry point
            Assert.True(MainWindow.GraphEditor.HasConnection(entryPoint, Port.Output(0), plusNodeModule, Port.Input(0)));
            // and the port type of the entry point is now "number"
            Assert.Equal((int)PortType.Number, entryPoint.GetPortType(Port.Output(0)));

        }
    }
}