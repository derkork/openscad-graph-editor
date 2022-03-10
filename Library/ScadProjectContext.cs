using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public class ScadProjectContext : ICanBeRendered
    {
        
        public readonly List<ScadInvokableContext> Modules = new List<ScadInvokableContext>();
        public readonly List<ScadInvokableContext> Functions = new List<ScadInvokableContext>();
        public ScadInvokableContext MainModule { get; private set; }

        public ScadProjectContext()
        {
            var mainModuleGraph = new LightWeightGraph();
            var startNode = new Start();
            mainModuleGraph.Blank("<main>", startNode);
            MainModule = new ScadInvokableContext(mainModuleGraph);
        }

        private void Clear()
        {
            Modules.ForAll(it => it.Discard());
            Functions.ForAll(it => it.Discard());
            MainModule.Discard();

            Modules.Clear();
            Functions.Clear();
        }

        public void Load(SavedProject project)
        {
            Clear();
            foreach (var function in project.Functions)
            {
                var functionContext = new ScadInvokableContext(new LightWeightGraph());
                functionContext.Load(function);
                Functions.Add(functionContext);
            }

            foreach (var module in project.Modules)
            {
                var moduleContext = new ScadInvokableContext(new LightWeightGraph());
                moduleContext.Load(module);
                Modules.Add(moduleContext);
            }

            MainModule = new ScadInvokableContext(new LightWeightGraph());
            MainModule.Load(project.MainModule);
        }

        public SavedProject Save()
        {
            var result = Prefabs.New<SavedProject>();
            Functions.Select(it => it.Save()).ForAll(it => result.Functions.Add(it));
            Modules.Select(it => it.Save()).ForAll(it => result.Modules.Add(it));
            result.MainModule = MainModule.Save();
            return result;
        }

        public string Render()
        {
            return string.Join("\n",
                Modules.Select(it => it.Render())
                    .Union(Functions.Select(it => it.Render()))
                    .Append(MainModule.Render())
                    .Where(it => it.Length > 0)
            );
        }

        public void Discard()
        {
            Modules.ForAll(it => it.Discard());
            Functions.ForAll(it => it.Discard());
            MainModule?.Discard();
        }
    }
}