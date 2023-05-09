using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using Serilog;

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
        private readonly Queue<Refactoring> _refactorings = new Queue<Refactoring>();

        /// <summary>
        /// Dictionary storing data under the ticket of the refactoring that created it.
        /// </summary>
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();


        public RefactoringContext(ScadProject project)
        {
            Project = project;
        }

        public void PerformRefactorings(IEnumerable<Refactoring> refactorings)
        {
            _refactorings.Clear();
            _data.Clear();
            refactorings.ForAll(_refactorings.Enqueue);


            while (_refactorings.Any())
            {
                var refactoring = _refactorings.Dequeue();
                Log.Information("Performing refactoring {Refactoring}", refactoring.GetType().Name);
                refactoring.PerformRefactoring(this);
            }
        }

        /// <summary>
        /// Can be called by refactorings, to run another refactoring. If the refactoring runs in late phase, it will
        /// be enqueued and run after all other refactorings have been applied. Otherwise it will be run immediately.
        /// </summary>
        /// <param name="refactoring"></param>
        internal void PerformRefactoring(Refactoring refactoring)
        {
            if (refactoring.IsLate)
            {
                Log.Information("Enqueuing late-stage refactoring {Refactoring}", refactoring.GetType().Name);
                _refactorings.Enqueue(refactoring);
                return;
            }

            Log.Information("Performing refactoring {Refactoring}", refactoring.GetType().Name);
            refactoring.PerformRefactoring(this);
        }

        /// <summary>
        /// Stores ticket data under the ticket of the refactoring that created it.
        /// </summary>
        internal void StoreTicketData<T>(IProvideTicketData<T> source, T data)
        {
            _data[source.Ticket] = data;
        }

        /// <summary>
        /// Tries to retrieve data stored under the ticket of the refactoring that created it.
        /// </summary>
        public bool TryGetData<T>(string ticket, out T data)
        {
            data = default;
            if (!_data.ContainsKey(ticket))
            {
                return false;
            }

            var result = _data[ticket];
            if (!(result is T t))
            {
                return false;
            }

            data = t;
            return true;
        }
    }
}