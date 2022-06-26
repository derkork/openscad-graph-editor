using System;
using System.Collections.Generic;

namespace OpenScadGraphEditor.Refactorings
{
    public interface ICanPerformRefactorings
    {
        /// <summary>
        /// Performs the given refactorings.
        /// </summary>
        void PerformRefactorings(string description, params Refactoring[] refactorings);

        /// <summary>
        /// Performs the given refactorings and executes the given actions afterwards.
        /// </summary>
        void PerformRefactorings(string description, IEnumerable<Refactoring> refactorings,
            params Action[] after);
    }
}