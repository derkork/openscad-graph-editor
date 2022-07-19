using System;
using Godot;
using GodotExt;
using Serilog.Core;
using Serilog.Events;

namespace OpenScadGraphEditor.Widgets.LogConsole
{
    public class LogConsole : Control, ILogEventSink
    {
        private RichTextLabel _logOutput;

        /// <summary>
        /// Event being raised when the user requests to open the log file.
        /// </summary>
        public event Action OpenLogFileRequested;
        
        public override void _Ready()
        {
            _logOutput = this.WithName<RichTextLabel>("LogOutput");
            this.WithName<IconButton.IconButton>("OpenLogButton").ButtonPressed += () => OpenLogFileRequested?.Invoke();
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