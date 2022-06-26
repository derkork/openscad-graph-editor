using System;
using System.Threading.Tasks;
using Godot;

namespace GodotXUnitApi
{
    /// <summary>
    /// helper methods for simulating mouse events.
    /// </summary>
    public static class GDI
    {
        /// <summary>
        /// gets the x pixel position based on the percent of the screen
        /// </summary>
        public static float PositionXByScreenPercent(float percent)
        {
            return GDU.Viewport.Size.x * percent;
        }
        
        /// <summary>
        /// gets the y pixel position based on the percent of the screen
        /// </summary>
        public static float PositionYByScreenPercent(float percent)
        {
            return GDU.Viewport.Size.y * percent;
        }
        
        /// <summary>
        /// gets a vector2 representing the pixel positions based on the screen percents. 
        /// </summary>
        public static Vector2 PositionByScreenPercent(float screenPercentX, float screenPercentY)
        {
            return new Vector2(PositionXByScreenPercent(screenPercentX), PositionYByScreenPercent(screenPercentY));
        }

        /// <summary>
        /// gets a vector2 representing the pixel positions based on the world position handed in.
        ///
        /// NOTE: i haven't tested this for 3d because i've only written 2d games with godot,
        /// so i've limited this method to 2d scenes only.
        /// </summary>
        public static Vector2 PositionBy2DWorldPos(Vector2 worldPos)
        {
            var scene = GDU.CurrentScene as Node2D;
            if (scene == null)
                throw new Exception("scene root must be a Node2D to use this method");
            return (scene.GetViewportTransform() * scene.GetGlobalTransform()).Xform(worldPos);
        }
        
        /// <summary>
        /// sends an mouse down event into godot
        /// </summary>
        public static void InputMouseDown(Vector2 screenPosition, ButtonList index = ButtonList.Left)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Position = screenPosition;
            inputEvent.Pressed = true;
            inputEvent.ButtonIndex = (int) index;
            inputEvent.ButtonMask = (int) index;
            Input.ParseInputEvent(inputEvent);
        }

        /// <summary>
        /// sends an mouse up event into godot
        /// </summary>
        public static void InputMouseUp(Vector2 screenPosition, ButtonList index = ButtonList.Left)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Position = screenPosition;
            inputEvent.Pressed = false;
            inputEvent.ButtonIndex = (int) index;
            inputEvent.ButtonMask = (int) index;
            Input.ParseInputEvent(inputEvent);
        }

        /// <summary>
        /// sends an mouse move event into godot
        /// </summary>
        public static void InputMouseMove(Vector2 screenPosition)
        {
            var inputEvent = new InputEventMouseMotion();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Position = screenPosition;
            Input.ParseInputEvent(inputEvent);
        }

        /// <summary>
        /// simulates a click with these steps:
        /// - send a mouse moved event to the requested position
        /// - send a mouse down event
        /// - send a mouse up event
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        /// </summary>
        /// <param name="screenPercentX">same as PositionByScreenPercent</param>
        /// <param name="screenPercentY">same as PositionByScreenPercent</param>
        /// <param name="index">the button index</param>
        /// <returns>the task that will resolve when the simulation is finished</returns>
        public static async Task SimulateMouseClick(float screenPercentX, float screenPercentY, ButtonList index = ButtonList.Left)
        {
            var position = PositionByScreenPercent(screenPercentX, screenPercentY);
            await SimulateMouseClick(position, index);
        }

        /// <summary>
        /// simulates a click with these steps:
        /// - send a mouse moved event to the requested position
        /// - send a mouse down event
        /// - send a mouse up event
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        /// </summary>
        /// <param name="screenPercentX">same as PositionByScreenPercent</param>
        /// <param name="screenPercentY">same as PositionByScreenPercent</param>
        /// <param name="index">the button index</param>
        public static void SimulateMouseClickNoWait(float screenPercentX, float screenPercentY, ButtonList index = ButtonList.Left)
        {
#pragma warning disable 4014
            SimulateMouseClick(screenPercentX, screenPercentY, index);
#pragma warning restore 4014
        }

        /// <summary>
        /// simulates a click with these steps:
        /// - send a mouse moved event to the requested position
        /// - send a mouse down event
        /// - send a mouse up event
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        ///
        /// the task will resolve after the simulation is completed.
        /// </summary>
        /// <param name="position">the position of where to click</param>
        /// <param name="index">the button index</param>
        /// <returns>the task that will resolve when the simulation is finished</returns>
        public static async Task SimulateMouseClick(Vector2 position, ButtonList index = ButtonList.Left)
        {
            await GDU.OnProcessAwaiter;
            InputMouseMove(position);
            await GDU.OnProcessAwaiter;
            InputMouseDown(position, index);
            await GDU.OnProcessAwaiter;
            await GDU.OnProcessAwaiter;
            InputMouseUp(position, index);
            await GDU.OnProcessAwaiter;
            await GDU.OnProcessAwaiter;
        }
        
        /// <summary>
        /// simulates a click with these steps:
        /// - send a mouse moved event to the requested position
        /// - send a mouse down event
        /// - send a mouse up event
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        /// </summary>
        /// <param name="position">the position of where to click</param>
        /// <param name="index">the button index</param>
        public static void SimulateMouseClickNoWait(Vector2 position, ButtonList index = ButtonList.Left)
        {
#pragma warning disable 4014
            SimulateMouseClick(position, index);
#pragma warning restore 4014
        }
        
        /// <summary>
        /// simulates a mouse drag with these steps:
        /// - move mouse to start position
        /// - send mouse down into godot
        /// - move mouse to end position
        /// - send mouse up into godot
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        /// </summary>
        /// <param name="startScreenPercentX">same as PositionByScreenPercent</param>
        /// <param name="startScreenPercentY">same as PositionByScreenPercent</param>
        /// <param name="endScreenPercentX">same as PositionByScreenPercent</param>
        /// <param name="endScreenPercentY">same as PositionByScreenPercent</param>
        /// <param name="index">the button index</param>
        /// <returns>the task that will resolve when the simulation is finished</returns>
        public static async Task SimulateMouseDrag(float startScreenPercentX, float startScreenPercentY,
                                                   float endScreenPercentX, float endScreenPercentY,
                                                   ButtonList index = ButtonList.Left)
        {
            var start = PositionByScreenPercent(startScreenPercentX, startScreenPercentY);
            var end = PositionByScreenPercent(endScreenPercentX, endScreenPercentY);
            await SimulateMouseDrag(start, end, index);
        }

        /// <summary>
        /// simulates a mouse drag with these steps:
        /// - move mouse to start position
        /// - send mouse down into godot
        /// - move mouse to end position
        /// - send mouse up into godot
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        /// </summary>
        /// <param name="startScreenPercentX">same as PositionByScreenPercent</param>
        /// <param name="startScreenPercentY">same as PositionByScreenPercent</param>
        /// <param name="endScreenPercentX">same as PositionByScreenPercent</param>
        /// <param name="endScreenPercentY">same as PositionByScreenPercent</param>
        /// <param name="index">the button index</param>
        public static void SimulateMouseDragNoWait(float startScreenPercentX, float startScreenPercentY,
                                                   float endScreenPercentX, float endScreenPercentY,
                                                   ButtonList index = ButtonList.Left)
        {
#pragma warning disable 4014
            SimulateMouseDrag(startScreenPercentX, startScreenPercentY, endScreenPercentX, endScreenPercentY, index);
#pragma warning restore 4014
        }

        /// <summary>
        /// simulates a mouse drag with these steps:
        /// - move mouse to start position
        /// - send mouse down into godot
        /// - move mouse to end position
        /// - send mouse up into godot
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        /// </summary>
        /// <param name="start">the position of where the drag starts</param>
        /// <param name="end">the position of where the drag ends</param>
        /// <param name="index">the button index</param>
        /// <returns>the task that will resolve when the simulation is finished</returns>
        public static async Task SimulateMouseDrag(Vector2 start, Vector2 end, ButtonList index = ButtonList.Left)
        {
            await GDU.OnProcessAwaiter;
            InputMouseMove(start);
            await GDU.OnProcessAwaiter;
            InputMouseDown(start, index);
            await GDU.OnProcessAwaiter;
            await GDU.OnProcessAwaiter;
            InputMouseMove(end);
            await GDU.OnProcessAwaiter;
            InputMouseUp(end, index);
            await GDU.OnProcessAwaiter;
            await GDU.OnProcessAwaiter;
        }

        /// <summary>
        /// simulates a mouse drag with these steps:
        /// - move mouse to start position
        /// - send mouse down into godot
        /// - move mouse to end position
        /// - send mouse up into godot
        ///
        /// at least a single frame is skipped between each event to make sure
        /// godot and the scene has time to react.
        /// </summary>
        /// <param name="start">the position of where the drag starts</param>
        /// <param name="end">the position of where the drag ends</param>
        /// <param name="index">the button index</param>
        public static void SimulateMouseDragNoWait(Vector2 start, Vector2 end, ButtonList index = ButtonList.Left)
        {
#pragma warning disable 4014
            SimulateMouseDrag(start, end, index);
#pragma warning restore 4014
        }
    }
}