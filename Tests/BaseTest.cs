using System;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver;
using GodotTestDriver.Util;
using GodotXUnitApi;
using Xunit;
using Thread = System.Threading.Thread;

namespace OpenScadGraphEditor.Tests
{
    // super important - this prevents the tests from running in parallel
    // if they would run in parallel, they would interfere with each other
    [Collection("Integration Tests")]
    public abstract class BaseTest : IAsyncLifetime
    {
        protected Fixture Fixture { get; } = new Fixture(GDU.Tree);

        protected virtual async Task Setup() { await Task.CompletedTask; }
        protected virtual async Task TearDown() { await Task.CompletedTask; }
        
        protected async Task WithinSeconds(float seconds, Action action)
        {
            PrintThread();
            await Fixture.Tree.WithinSeconds(seconds, () =>
            {
                GD.Print("in action");
                PrintThread();
                action();
            });
        }
        
        protected async Task DuringSeconds(float seconds, Action action)
        {
            await Fixture.Tree.DuringSeconds(seconds, action);
        }
        
        public async Task InitializeAsync()
        {
            await Setup();
        }


        public static void PrintThread()
        {
            GD.Print("Thread " + Thread.CurrentThread.ManagedThreadId);
        }
        
        
        
        public async Task DisposeAsync()
        {
            await Fixture.Tree.ProcessFrame();
            await TearDown();
            GD.Print("Fixture cleanup starts");
            await Fixture.Cleanup();
            GD.Print("Fixture cleanup done");
        }
    }
}