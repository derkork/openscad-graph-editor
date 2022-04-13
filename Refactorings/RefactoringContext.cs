using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// This class represents a context in which refactoring operations can work. It provides editable
    /// graphs for refactorings to work on and ensures that after the refactorings are applied any modified
    /// state is updated in the UI.
    /// </summary>
    public class RefactoringContext
    {
        public ScadProject Project { get; }
        private readonly List<Refactoring> _refactorings = new List<Refactoring>();

        private readonly Dictionary<IScadGraph, IScadGraph> _modifiedVisibleGraphs =
            new Dictionary<IScadGraph, IScadGraph>();

        private RefactoringPhase _phase;

        /// <summary>
        /// An enum representing the current refactoring phase.
        /// </summary>
        private enum RefactoringPhase
        {
            /// <summary>
            /// The default phase. Most refactorings run in this phase.
            /// </summary>
            Default,
            
            /// <summary>
            /// The late phase. Refactorings in this phase are applied after all other refactorings have been.
            /// </summary>
            Late
        }


        public RefactoringContext(ScadProject project)
        {
            Project = project;
            _phase = RefactoringPhase.Default;
        }

        public void PerformRefactorings(IEnumerable<Refactoring> refactorings, params Action[] after )
        {
            _refactorings.AddRange(refactorings);
            
            // start with the default phase.
            var defaultPhaseRefactorings = _refactorings.Where(it => !it.IsLate)
                .ToList();

            foreach (var refactoring in defaultPhaseRefactorings)
            {
                GD.Print("Performing refactoring: " + refactoring.GetType().Name);
                refactoring.PerformRefactoring(this);
            }

            // now the late phase
            _phase = RefactoringPhase.Late;

            var latePhaseRefactorings = _refactorings.Where(it => it.IsLate)
                .ToList();
            foreach (var refactoring in latePhaseRefactorings)
            {
                GD.Print("Performing late-stage refactoring: " + refactoring.GetType().Name);
                refactoring.PerformRefactoring(this);
            }
         

            TransferModifiedDataBackToVisibleGraphs();
            foreach (var afterAction in after)
            {
                afterAction();
            }
            _refactorings.Clear();
        }

        /// <summary>
        /// Can be called by refactorings, to run another refactoring. If the refactoring's phase matches
        /// the current phase it will be performed immediately, otherwise it will be performed later
        /// when the phase of th refactoring comes. 
        /// </summary>
        /// <param name="refactoring"></param>
        internal void PerformRefactoring(Refactoring refactoring)
        {
            if (_phase == RefactoringPhase.Default && refactoring.IsLate)
            {
                _refactorings.Add(refactoring);
                return;
            } 
            
            refactoring.PerformRefactoring(this);
        }
        
        /// <summary>
        /// Makes a graph refactorable (e.g. transforms it into a LightWeightGraph). We don't perform refactorings
        /// on heavyweight UI graphs because that would introduce all kinds of rendering headache. Instead we
        /// perform refactorings on LightWeightGraphs and then transform the refactored graph back into a visual
        /// representation.
        /// </summary>
        internal LightWeightGraph MakeRefactorable(IScadGraph graph)
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


        internal void MarkDeleted(IScadGraph graph)
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
        private void TransferModifiedDataBackToVisibleGraphs()
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