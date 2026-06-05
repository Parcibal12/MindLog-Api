namespace MindLog.Api.Core.Domain.Entities
{
    public class EntryContext
    {
        public Guid EntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }

        public int TagId { get; set; }
        public ContextTag? ContextTag { get; set; }
    }
}