namespace MindLog.Api.Core.Domain.Interfaces
{
    public interface IClinicalReportService
    {
        Task ProcessAutomaticReportsAsync();
    }
}