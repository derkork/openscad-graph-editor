using System;
using Godot;
using GodotTestDriver.Drivers;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Tests.Drivers
{
    /// <summary>
    /// Driver for a <see cref="CheckBox"/>.
    /// </summary>
    [PublicAPI]
    public class CheckBoxDriver<T> : ButtonDriver<T> where T:CheckBox
    {
        public CheckBoxDriver(Func<T> provider, string description = "") : base(provider, description)
        {
        }
        
        public bool IsChecked => PresentRoot.Pressed;
    }
    
    
    /// <summary>
    /// Driver for a <see cref="CheckBox"/>.
    /// </summary>
    [PublicAPI]    
    public sealed class CheckBoxDriver : CheckBoxDriver<CheckBox>
    {
        public CheckBoxDriver(Func<CheckBox> provider, string description = "") : base(provider, description)
        {
        }
    }
}