using Godot;
using GodotExt;
using Serilog.Core;
using Serilog.Events;

namespace OpenScadGraphEditor.Widgets.LogConsole
{
    public class LogConsole : Control, ILogEventSink
    {
        private RichTextLabel _logOutput;
        
        
        public override void _Ready()
        {
            _logOutput = this.WithName<RichTextLabel>("LogOutput");
        }
        
        public void Open()
        {
            Visible = true;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = $"{logEvent.Timestamp.ToString()} {logEvent.RenderMessage()}\n";
            _logOutput.AppendBbcode(message);

            while (_logOutput.GetLineCount() > 500)
            {
                _logOutput.RemoveLine(0);
            }
            
        }
    }
}