namespace MindLog.Api.Core.Domain.Entities
{
    public class JournalEntry
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public int EmotionId { get; set; }
        public int Intensity { get; set; }
        public string? AiFeedback { get; set; }
        public string? AiPattern { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User? User { get; set; }
        public Emotion? Emotion { get; set; }
        public ICollection<EntryContext> EntryContexts { get; set; } = new List<EntryContext>();
    }
}