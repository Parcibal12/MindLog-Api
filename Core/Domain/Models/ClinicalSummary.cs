namespace MindLog.Api.Core.Domain.Models
{
    public class ClinicalSummary
    {
        public Guid UserId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PsychologistEmail { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalEntriesRecorded { get; set; }
        public string DominantEmotion { get; set; } = string.Empty;
        public string DominantCognitivePattern { get; set; } = string.Empty;
        
        public List<CriticalEntry> CriticalEntries { get; set; } = new();
    }

    public class CriticalEntry
    {
        public DateTime Date { get; set; }
        public string Emotion { get; set; } = string.Empty;
        public int Intensity { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AiPattern { get; set; } = string.Empty;
    }
}