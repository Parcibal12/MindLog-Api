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
    public async Task<int> GetCurrentStreakAsync(Guid userId)
        {
            var entryDates = await _repository.GetEntryDatesAsync(userId);

            if (!entryDates.Any()) return 0;

            int streak = 0;
            var today = DateTime.UtcNow.Date;
            var lastEntryDate = entryDates.First();

            if (lastEntryDate < today.AddDays(-1))
            {
                return 0;
            }

            var dateToCheck = lastEntryDate == today ? today : today.AddDays(-1);

            foreach (var date in entryDates)
            {
                if (date == dateToCheck)
                {
                    streak++;
                    dateToCheck = dateToCheck.AddDays(-1); 
                }
                else if (date < dateToCheck)
                {
                    break;
                }
            }

            return streak;
        }
    }
}