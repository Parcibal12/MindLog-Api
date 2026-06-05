using MindLog.Api.Core.Domain.Entities;

namespace MindLog.Api.Core.Domain.Interfaces
{
    public interface IJournalEntryRepository
    {
        Task<JournalEntry?> GetByIdAsync(Guid id);
        Task<IEnumerable<JournalEntry>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<JournalEntry>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<JournalEntry> AddAsync(JournalEntry entry);
        Task UpdateAsync(JournalEntry entry);
        Task DeleteAsync(Guid id);
    }
}