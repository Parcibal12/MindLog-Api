namespace MindLog.Api.Core.Domain.Interfaces
{
    public class AiAnalysisResult
    {
        public string Feedback { get; set; } = string.Empty;
        public string Pattern { get; set; } = "NEUTRAL";
        public int EmotionId { get; set; }
        public int Intensity { get; set; }
        public List<int> ContextTagIds { get; set; } = new();
    }

    public interface IAiFeedbackService
    {
        Task<AiAnalysisResult> AnalyzeJournalAsync(string content);
    }
}