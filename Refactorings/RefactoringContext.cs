using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// This class represents a context in which refactoring operations can work. It provides editable
    /// graphs for refactorings to work on and ensures that after the refactorings are applied any modified
    /// state is updated in the UI.
    /// </summary>
    public class RefactoringContext
    {
        private readonly GraphEditor _editor;
        public ScadProject Project { get; }
        private readonly List<Refactoring> _refactorings = new List<Refactoring>();

        private readonly Dictionary<IScadGraph, IScadGraph> _modifiedVisibleGraphs =
            new Dictionary<IScadGraph, IScadGraph>();


        public RefactoringContext(GraphEditor editor, ScadProject project)
        {
            _editor = editor;
            Project = project;
        }

        public void AddRefactoring(Refactoring refactoring)
        {
            _refactorings.Add(refactoring);
        }

        public void PerformRefactorings()
        {
            _refactorings.Where(it => !it.IsLate).ForAll(it =>
            {
                GD.Print("Performing refactoring: " + it.GetType().Name);
                it.PerformRefactoring(this);
            });

            // perform late-stage refactorings
            _refactorings.Where(it => it.IsLate).ForAll(it =>
            {
                GD.Print("Performing late-stage refactoring: " + it.GetType().Name);
                it.PerformRefactoring(this);
            });

            TransferModifiedDataBackToVisibleGraphs();
            _refactorings.ForAll(it => it.AfterRefactoring(_editor));
        }

        /// <summary>
        /// Makes a graph refactorable (e.g. transforms it into a LightWeightGraph). We don't perform refactorings
        /// on heavyweight UI graphs because that would introduce all kinds of rendering headache. Instead we
        /// perform refactorings on LightWeightGraphs and then transform the refactored graph back into a visual
        /// representation.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public LightWeightGraph MakeRefactorable(IScadGraph graph)
        {
            if (graph is LightWeightGraph lightWeightGraph)
            {
                return lightWeightGraph;
            }

            // when multiple refactorings are done on the same source graph, don't swap out the graph again.
            foreach (var entry in _modifiedVisibleGraphs.Where(entry => entry.Value == graph))
            {
                return entry.Key as LightWeightGraph;
            }

            var result = new LightWeightGraph();
            Project.TransferData(graph, result);
            _modifiedVisibleGraphs[result] = graph;
            return result;
        }


        public void MarkDeleted(IScadGraph graph)
        {
            GdAssert.That(graph is LightWeightGraph, "Only LightWeightGraphs can be marked as deleted.");
            // if the graph is currently visible, discard the visible graph as well
            if (_modifiedVisibleGraphs.TryGetValue(graph, out var visibleGraph))
            {
                visibleGraph.Discard();
            }

            _modifiedVisibleGraphs.Remove(graph);
        }
        
        /// <summary>
        /// Transforms all lightweight graphs which represent graphs that are currently open for editing back into
        /// heavyweight graphs.
        /// </summary>
        public void TransferModifiedDataBackToVisibleGraphs()
        {
            foreach (var lightWeightGraph in _modifiedVisibleGraphs.Keys)
            {
                var heavyWeightGraph = _modifiedVisibleGraphs[lightWeightGraph];
                Project.TransferData(lightWeightGraph, heavyWeightGraph);
                lightWeightGraph.Discard();
            }

            _modifiedVisibleGraphs.Clear();
        }
    }
}