using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using GodotTestDriver.Util;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Tests.Drivers
{
    /// <summary>
    /// A driver for the <see cref="TabContainer"/> control.
    /// </summary>
    [PublicAPI]
    public class TabContainerDriver<T> : ControlDriver<T> where T : TabContainer
    {
        public TabContainerDriver(Func<T> producer, string description = "") : base(producer, description)
        {
        }


        /// <summary>
        /// The amount of tabs currently open in the tab control.
        /// </summary>
        public int TabCount => PresentRoot.GetTabCount();

        /// <summary>
        /// Returns the titles of the currently open tabs.
        /// </summary>
        public IEnumerable<string> TabTitles
        {
            get
            {
                for (var i = 0; i < PresentRoot.GetTabCount(); i++)
                {
                    yield return PresentRoot.GetTabTitle(i);
                }
            }
        }

        /// <summary>
        /// Returns the tab index that is currently selected.
        /// </summary>
        public int SelectedTabIndex => PresentRoot.CurrentTab;

        /// <summary>
        /// Returns the title of the currently selected tab.
        /// </summary>
        public string SelectedTabTitle => PresentRoot.GetTabTitle(SelectedTabIndex);


        /// <summary>
        /// Selects the tab with the given index.
        /// </summary>
        /// <param name="index"></param>
        public async Task SelectTab(int index)
        {
            var tab = VisibleRoot;

            if (index < 0 || index >= tab.GetTabCount())
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    "Index must be between 0 and the amount of tabs in the tab control.");
            }

            await tab.GetTree().ProcessFrame();
            var previousTab = tab.CurrentTab;

            tab.CurrentTab = index;

            // emit the signals for the tab change
            tab.EmitSignal("tab_selected", index);
            if (previousTab != index)
            {
                tab.EmitSignal("tab_changed", index);
            }

            await tab.GetTree().WaitForEvents();
        }

        /// <summary>
        /// Selects the tab with the given title
        /// </summary>
        public async Task SelectTab(string title)
        {
            var index = TabTitles.ToList().IndexOf(title);
            if (index < 0)
            {
                throw new ArgumentException($"No tab with the title '{title}' was found.", nameof(title));
            }

            await SelectTab(index);
        }
    }
    
    [PublicAPI]
    public sealed class TabContainerDriver : TabContainerDriver<TabContainer>
    {
        public TabContainerDriver(Func<TabContainer> producer, string description = "") : base(producer, description)
        {
        }
    }
}