namespace MindLog.Api.Core.Application.Services.DTOs
{
    public class UpdateProfileDto
    {
        public string? TherapistName { get; set; }
        public string? TherapistEmail { get; set; }
        public bool AutoSendReports { get; set; }
    }
}