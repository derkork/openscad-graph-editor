using Godot;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// Wrapper class to get our own data in and out of Godot land.
    /// </summary>
    public class DragData : Reference
    {
        private readonly object _data;

        public DragData(object data)
        {
            _data = data;
        }
        
        public bool TryGetData<T>(out T data)
        {
            if (_data is T t)
            {
                data = t;
                return true;
            }

            data = default;
            return false;
        }
    }
}