using System;

namespace OpenScadGraphEditor.Library.IO
{
    public class BrokenFileException : Exception
    {
        public BrokenFileException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}