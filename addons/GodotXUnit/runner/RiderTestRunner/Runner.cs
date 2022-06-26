using System;
using System.Linq;
using Godot;
using GodotXUnitApi;
using GodotXUnitApi.Internal;
using Environment = System.Environment;
using Thread = System.Threading.Thread;

// ReSharper disable once CheckNamespace
namespace RiderTestRunner
{
    // ReSharper disable once UnusedType.Global
    public class Runner : GodotXUnitRunnerBase // for GodotXUnit use: public class Runner : GodotTestRunner. https://github.com/fledware/GodotXUnit/issues/8#issuecomment-929849478
    {
        public override void _Ready()
        {
            GDU.Instance = this; // for GodotXUnit https://github.com/fledware/GodotXUnit/issues/8#issuecomment-929849478
            var textNode = GetNode<RichTextLabel>("RichTextLabel");
            foreach (var arg in OS.GetCmdlineArgs())
            {
                textNode.Text += Environment.NewLine + arg;
            }

            if (OS.GetCmdlineArgs().Length < 4)
                return;

            var unitTestAssembly = OS.GetCmdlineArgs()[2];
            var unitTestArgs = OS.GetCmdlineArgs()[4].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToArray();
            // https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.executeassembly?view=netframework-4.7.2
            AppDomain currentDomain = AppDomain.CurrentDomain;
            var thread = new Thread(() =>
            {
                currentDomain.ExecuteAssembly(unitTestAssembly, unitTestArgs);
                GetTree().Quit();
            });
            thread.Start();

            WaitForThreadExit(thread);
        }

        private async void WaitForThreadExit(Thread thread)
        {
            while (thread.IsAlive)
            {
                await ToSignal(GetTree().CreateTimer(1), "timeout");
            }
        }
    }
}