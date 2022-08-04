using System;
using System.Linq;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class Vector3EditDriver : ControlDriver<Vector3Edit>
    {
        public LineEditDriver X { get; }
        public LineEditDriver Y { get; }
        public LineEditDriver Z { get; }
        
        public Vector3EditDriver(Func<Vector3Edit> producer, string description = "") : base(producer, description)
        {
            X = new LineEditDriver(() => Root?.GetChildNodes<GridContainer>().FirstOrDefault()?.GetChildNodes<LineEdit>().FirstOrDefault(), Description + " -> X");
            Y = new LineEditDriver(() => Root?.GetChildNodes<GridContainer>().FirstOrDefault()?.GetChildNodes<LineEdit>().Skip(1).FirstOrDefault(), Description + " -> Y");
            Z = new LineEditDriver(() => Root?.GetChildNodes<GridContainer>().FirstOrDefault()?.GetChildNodes<LineEdit>().Skip(2).FirstOrDefault(), Description + " -> Z");
        }
    }
}