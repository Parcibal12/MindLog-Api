using MindLog.Api.Core.Domain.Entities;

namespace MindLog.Api.Core.Application.Services
{
    public interface IJournalService
    {
        Task<JournalEntry> CreateJournalEntryAsync(JournalEntry entry, string emotionName);
    }
}