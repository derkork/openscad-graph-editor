using System;
using System.Threading.Tasks;
using GodotTestDriver;
using GodotTestDriver.Util;
using GodotXUnitApi;
using JetBrains.Annotations;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public abstract class BaseTest : IAsyncLifetime
    {
        protected Fixture Fixture { get; } = new Fixture(GDU.Tree);

        protected virtual async Task Setup() { await Task.CompletedTask; }
        protected virtual async Task TearDown() { await Task.CompletedTask; }
        
        protected static async Task WithinSeconds(float seconds, Action action)
        {
            await GDU.Tree.WithinSeconds(seconds, action);
        }
        
        protected static async Task DuringSeconds(float seconds, Action action)
        {
            await GDU.Tree.DuringSeconds(seconds, action);
        }
        
        public async Task InitializeAsync()
        {
            await Setup();
        }
        
        
        
        
        public async Task DisposeAsync()
        {
            await TearDown();
            await Fixture.Cleanup();
        }
    }
}