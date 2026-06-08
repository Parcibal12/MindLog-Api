using MindLog.Api.Core.Domain.Models;

namespace MindLog.Api.Core.Domain.Interfaces
{
    public interface IReportGenerator
    {
        Task<byte[]> GenerateClinicalPdfAsync(ClinicalSummary summary);
    }
}