using System.Collections.Generic;
using Godot;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactoring
{
    public abstract class Refactoring : Reference
    {
        /// <summary>
        /// Performs the actual refactoring by modifying the the given project. All graphs in the project
        /// are guaranteed to be LightWeightGraphs when this is invoked.
        /// </summary>
        public abstract void PerformRefactoring(ScadProject project);

        /// <summary>
        /// Called after the refactoring is applied. Can be used to open certain items in the graph editor.
        /// </summary>
        /// <param name="graphEditor"></param>
        public virtual void AfterRefactoring(GraphEditor graphEditor)
        {
        }


        private readonly Dictionary<IScadGraph, IScadGraph> _modifiedVisibleGraphs = new Dictionary<IScadGraph, IScadGraph>();

        /// <summary>
        /// Makes a graph refactorable (e.g. transforms it into a LightWeightGraph). We don't perform refactorings
        /// on heavyweight UI graphs because that would introduce all kinds of rendering headache. Instead we
        /// perform refactorings on LightWeightGraphs and then transform the refactored graph back into a visual
        /// representation.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        protected LightWeightGraph MakeRefactorable(IScadGraph graph, ScadProject project)
        {
            if (graph is LightWeightGraph lightWeightGraph)
            {
                return lightWeightGraph;
            }

            var result = new LightWeightGraph();
            project.TransferData(graph, result);
            _modifiedVisibleGraphs[result] = graph;
            return result;
        }

        /// <summary>
        /// Transforms all lightweight graphs which represent graphs that are currently open for editing back into
        /// heavyweight graphs.
        /// </summary>
        protected void TransferModifiedDataBackToVisibleGraphs(ScadProject scadProject)
        {
            foreach (var lightWeightGraph in _modifiedVisibleGraphs.Keys)
            {
                var heavyWeightGraph = _modifiedVisibleGraphs[lightWeightGraph];
                scadProject.TransferData(lightWeightGraph, heavyWeightGraph);
                lightWeightGraph.Discard();
            }
            
            _modifiedVisibleGraphs.Clear();
        }
    }
}