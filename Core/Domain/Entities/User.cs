namespace MindLog.Api.Core.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PinHash { get; set; } = string.Empty;
        public string? TherapistName { get; set; }
        public string? TherapistEmail { get; set; }
        public bool AutoSendReports { get; set; }
        public bool BiometricEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }
}