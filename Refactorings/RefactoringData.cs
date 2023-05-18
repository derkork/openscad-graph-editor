using System.Collections.Generic;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// This class represents data produced by a refactoring. It can be used to perform additional actions
    /// after a refactoring.
    /// </summary>
    public class RefactoringData
    {
        /// <summary>
        /// Dictionary storing data under the ticket of the refactoring that created it.
        /// </summary>
        private readonly Dictionary<object, object> _data = new Dictionary<object, object>();
        
        /// <summary>
        /// Tries to retrieve data stored under the ticket of the refactoring that created it.
        /// </summary>
        public bool TryGetData<TV>(RefactoringDataKey<TV> key, out TV data) 
        {
            data = default;
            if (!_data.ContainsKey(key))
            {
                return false;
            }

            var result = _data[key];
            if (!(result is TV v))
            {
                return false;
            }

            data = v;
            return true;
        }
        
        /// <summary>
        /// Stores refactoring data under given key.
        /// </summary>
        public void StoreData<TV>(RefactoringDataKey<TV> key, TV data) 
        {
            _data[key] = data;
        }

        public void Clear()
        {
            _data.Clear();
        }
    }
}