namespace MindLog.Api.Core.Domain.Interfaces
{
    public interface IAiFeedbackService
    {
        Task<(string Feedback, string Pattern)> AnalyzeJournalAsync(string content, string emotionName);
    }
}