namespace MindLog.Api.Core.Domain.Entities
{
    public class Emotion
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ColorHex { get; set; }

        public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }
}