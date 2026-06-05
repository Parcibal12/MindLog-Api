namespace MindLog.Api.Core.Application.DTOs
{
    public class CreateJournalDto
    {
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int EmotionId { get; set; }
        public string EmotionName { get; set; } = string.Empty;
        public int Intensity { get; set; }
        public List<int> ContextTagIds { get; set; } = new List<int>();
    }
}