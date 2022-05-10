using System.Collections.Generic;
using System.Linq;
using Godot;
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

        public void PerformRefactorings(IEnumerable<Refactoring> refactorings)
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
            
            GD.Print("Performing refactoring: " + refactoring.GetType().Name);
            refactoring.PerformRefactoring(this);
        }

    }
}