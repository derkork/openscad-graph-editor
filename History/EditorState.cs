using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.History
{
    /// <summary>
    /// This holds the state of the editor at a certain point in time.
    /// </summary>
    public class EditorState
    {
       public List<EditorOpenTab> OpenTabs { get; }

       public EditorState(IEnumerable<EditorOpenTab> openTabs)
       {
           OpenTabs = openTabs.ToList();
       }
    }
}