using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using GodotXUnitApi.Internal;

namespace GodotXUnitApi
{
    /// <summary>
    /// a global helper for interacting with the executing tree
    /// during a test.
    /// </summary>
    // here are a few usage examples:
    //
    // - get the current scene:
    // var scene = GDU.CurrentScene;
    //
    // - wait for 60 process frames
    // await GDU.WaitForFrames(60);
    //
    // - move to the physics frame
    // await GDU.OnPhysicsProcessAwaiter;
    public static class GDU
    {
        private static Node2D _instance;

        public static Node2D Instance
        {
            get => _instance ?? throw new Exception("GDU not set");
            set => _instance = value;
        }

        public static SignalAwaiter OnProcessAwaiter =>
            Instance.ToSignal(Instance, "OnProcess");

        public static SignalAwaiter OnPhysicsProcessAwaiter =>
            Instance.ToSignal(Instance, "OnPhysicsProcess");

        public static SignalAwaiter OnIdleFrameAwaiter =>
            Instance.ToSignal(Instance.GetTree(), "idle_frame");

        public static SceneTree Tree => Instance.GetTree();

        public static Viewport Viewport => Instance.GetViewport();
        
        public static Node CurrentScene => Instance.GetTree().CurrentScene;

        /// <summary>
        /// this can be used within tests instead of grabbing ITestOutputHelper
        /// </summary>
        /// <param name="message"></param>
        public static void Print(string message)
        {
            // when [GodotFact] is used, the console output stream is
            // automatically overridden for each test. but this will
            // avoid the annoying warnings.
            Console.WriteLine(message);
        }

        /// <summary>
        /// creates a task the awaits for the given amount of _Process frames to happen
        /// </summary>
        /// <param name="count">the amount of frames to wait</param>
        /// <returns>the task that resolves after the amount of frames</returns>
        public static async Task WaitForFrames(int count)
        {
            for (int i = 0; i < count; i++)
                await OnProcessAwaiter;
        }
        
        /// <summary>
        /// helper to wrap a SignalAwaiter to return the first element from a signal
        /// result into the desired type.
        /// </summary>
        /// <param name="awaiter">the target signal to wrap</param>
        /// <typeparam name="T">the type to cast to</typeparam>
        /// <returns>the task that awaits and casts when resolved</returns>
        public static async Task<T> AwaitType<T>(this SignalAwaiter awaiter)
        {
            return (T) (await awaiter)[0];
        }
        
        /// <summary>
        /// creates a task for a godot signal with a timeout.
        /// </summary>
        /// <param name="source">the object that emits the signal</param>
        /// <param name="signal">the signal to wait for</param>
        /// <param name="timeoutMillis">the amount of millis before a timeout happens</param>
        /// <param name="throwOnTimeout">makes this task throw an exception on timeout. otherwise, just resolves</param>
        /// <returns>the new task with the given timeout</returns>
        /// <exception cref="TimeoutException">only throws if throwOnTimeout is true</exception>
        public static async Task<object[]> ToSignalWithTimeout(
            this Godot.Object source,
            string signal,
            int timeoutMillis,
            bool throwOnTimeout = true)
        {
            return await source.ToSignal(source, signal).AwaitWithTimeout(timeoutMillis, throwOnTimeout);
        }
        
        /// <summary>
        /// wraps the given SignalAwaiter in a task with a timeout.
        /// </summary>
        /// <param name="awaiter">the signal to add a timeout to</param>
        /// <param name="timeoutMillis">the amount of millis before a timeout happens</param>
        /// <param name="throwOnTimeout">makes this task throw an exception on timeout. otherwise, just resolves</param>
        /// <returns>the new task with the given timeout</returns>
        /// <exception cref="TimeoutException">only throws if throwOnTimeout is true</exception>
        public static Task<object[]> AwaitWithTimeout(
            this SignalAwaiter awaiter,
            int timeoutMillis,
            bool throwOnTimeout = true)
        {
            return Task.Run(async () => await awaiter).AwaitWithTimeout(timeoutMillis, throwOnTimeout);
        }
        
        /// <summary>
        /// wraps a task with a task that will resolve after the wrapped task
        /// or after the specified amount of time (either by exiting or by throwing
        /// an exception) 
        /// </summary>
        /// <param name="wrapping">the task to wrap</param>
        /// <param name="timeoutMillis">the amount of millis before a timeout happens</param>
        /// <param name="throwOnTimeout">makes this task throw an exception on timeout. otherwise, just resolves</param>
        /// <returns>the new task with the given timeout</returns>
        /// <exception cref="TimeoutException">only throws if throwOnTimeout is true</exception>
        public static async Task AwaitWithTimeout(
            this Task wrapping,
            int timeoutMillis,
            bool throwOnTimeout = true)
        {
            var task = Task.Run(async () => await wrapping);
            using var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMillis, token.Token));
            if (completedTask == task) {
                token.Cancel();
                await task;
            }
            if (throwOnTimeout)
                throw new TimeoutException($"signal {wrapping} timed out after {timeoutMillis}ms.");
        }
        
        /// <summary>
        /// wraps a task with a task that will resolve after the wrapped task
        /// or after the specified amount of time (either by exiting or by throwing
        /// an exception) 
        /// </summary>
        /// <param name="wrapping">the task to wrap</param>
        /// <param name="timeoutMillis">the amount of millis before a timeout happens</param>
        /// <param name="throwOnTimeout">makes this task throw an exception on timeout. otherwise, just resolves</param>
        /// <typeparam name="T">the wrapping type that will be the result if wrapping resolves</typeparam>
        /// <returns>the new task with the given timeout</returns>
        /// <exception cref="TimeoutException">only throws if throwOnTimeout is true</exception>
        public static async Task<T> AwaitWithTimeout<T>(
            this Task<T> wrapping,
            int timeoutMillis,
            bool throwOnTimeout = true) 
        {
            var task = Task.Run(async () => await wrapping);
            using var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMillis, token.Token));
            if (completedTask == task) {
                token.Cancel();
                return await task;
            }
            if (throwOnTimeout)
                throw new TimeoutException($"signal {wrapping} timed out after {timeoutMillis}ms.");
            return default;
        }

        /// <summary>
        /// allows the caller to draw for the specific amount of frames.
        /// 
        /// very helpful when trying to debug why the freaking buttons are not
        /// being clicked when you very clearly asked GDI to click something.
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="drawer"></param>
        /// <returns></returns>
        public static async Task RequestDrawing(int frames, Action<Node2D> drawer)
        {
            for (int i = 0; i < frames; i++)
            {
                ((GodotXUnitRunnerBase) Instance).RequestDraw(drawer);
                await Instance.ToSignal(Instance, "OnDrawRequestDone");
            }
        }
    }
}