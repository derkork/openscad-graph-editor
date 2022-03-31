using System.Collections.Generic;
using System.Linq;

namespace OpenScadGraphEditor.Library
{
    public static class ScadProjectExt
    {
        public static IEnumerable<IScadGraph> FindContainingReferencesTo(this ScadProject project,
            InvokableDescription invokableDescription)
        {
            var graphs = project.Functions.Concat(project.Modules).Append(project.MainModule);
            return graphs.Where(it => it.ContainsReferencesTo(invokableDescription));
        }
    }
}