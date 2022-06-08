using Xunit;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// use this like the standard Fact attribute. GodotFact can be used
    /// in plain unit tests (outside of the godot runtime) and with
    /// integration tests.
    ///
    /// Though, if you run tests outside of the godot runtime, godot methods
    /// will not be available. GDU.Instance will not be setup and the test
    /// setup will fail if using Scene or Frame.
    ///
    /// The main thing this will give you if using this outside of the godot
    /// runtime is grabbing console output as test output. this is useful
    /// when trying to debug non-godot class unit tests.
    ///
    /// do not use GodotFact outside of the godot runtime if you are
    /// running the tests asynchronously.
    /// </summary>
    /*
        [GodotFact]
        public void SomeNormalUnitTest()
        {
            Assert.True(false);
        }
        
        [GodotFact(Scene = "res://test_scenes/SomeTestScene.tscn")]
        public void IsOnCorrectScene()
        {
            var scene = GDU.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }
        
        [GodotFact(Frame = GodotFactFrame.Process)]
        public void ILikeToRunOnProcess()
        {
            GD.Print("i'm in the process event!!");
        }
        
        [GodotFact(Frame = GodotFactFrame.PhysicsProcess)]
        public void IsInPhysicsProcess()
        {
            Assert.True(Engine.IsInPhysicsFrame());
        }
     */
    [XunitTestCaseDiscoverer("GodotXUnitApi.Internal.GodotFactDiscoverer", "GodotXUnitApi")]
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class GodotFactAttribute : FactAttribute
    {
        /// <summary>
        /// loads the given scene before the test starts and loads an empty scene after
        /// the test is finished. the string must start with res://.. 
        ///
        /// NOTE: this cannot be used outside of the godot runtime
        /// </summary>
        public virtual string Scene { get; set; }

        /// <summary>
        /// changes the frame that the test is run in.
        ///
        /// NOTE: this cannot be used outside of the godot runtime
        /// </summary>
        public virtual GodotFactFrame Frame { get; set; } = GodotFactFrame.Default;
    }

    /// <summary>
    /// the frame to run the test in
    /// </summary>
    public enum GodotFactFrame
    {
        /// <summary>
        /// does not change the thread context before starting the test.
        /// note that the test thread can be in any of these frames.
        /// </summary>
        Default = 0,
        /// <summary>
        /// waits for a process frame before running the test
        /// </summary>
        Process = 1,
        /// <summary>
        /// waits for a physics process frame before running the test
        /// </summary>
        PhysicsProcess = 2
    }
}