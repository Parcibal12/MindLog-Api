using MindLog.Api.Core.Domain.Entities;
using MindLog.Api.Core.Domain.Interfaces;

namespace MindLog.Api.Core.Application.Services
{
    public class JournalService : IJournalService
    {
        private readonly IJournalEntryRepository _repository;

        public JournalService(IJournalEntryRepository repository)
        {
            _repository = repository;
        }

        public async Task<JournalEntry> CreateJournalEntryAsync(JournalEntry entry, string emotionName)
        {
                        
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;

            return await _repository.AddAsync(entry);
        }
    }
}