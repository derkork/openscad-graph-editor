namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Interface for refactorings that provide ticket data.
    /// </summary>
    public interface IProvideTicketData<T>
    {
        public string Ticket { get; }
    }
}