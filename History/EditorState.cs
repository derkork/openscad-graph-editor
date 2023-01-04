using System.Collections.Generic;
using System.Linq;
using GodotExt;

namespace OpenScadGraphEditor.History
{
    /// <summary>
    /// This holds the state of the editor at a certain point in time.
    /// </summary>
    public class EditorState
    {
       public List<EditorOpenTab> OpenTabs { get; }
       
       public int CurrentTabIndex { get; }

       public EditorState(IEnumerable<EditorOpenTab> openTabs, int currentTabIndex)
       {
           OpenTabs = openTabs.ToList();
           GdAssert.That(currentTabIndex >= 0 && currentTabIndex < OpenTabs.Count, $"currentTabIndex is out of range (0 <= {currentTabIndex} < {OpenTabs.Count})");
           CurrentTabIndex = currentTabIndex;
       }
    }
}