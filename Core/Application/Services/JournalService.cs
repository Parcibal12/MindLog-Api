using MindLog.Api.Core.Domain.Entities;
using MindLog.Api.Core.Domain.Interfaces;

namespace MindLog.Api.Core.Application.Services
{
    public class JournalService : IJournalService
    {
        private readonly IJournalEntryRepository _repository;
        private readonly IAiFeedbackService _aiService;

        public JournalService(IJournalEntryRepository repository, IAiFeedbackService aiService)
        {
            _repository = repository;
            _aiService = aiService;
        }

        public async Task<JournalEntry> CreateJournalEntryAsync(JournalEntry entry, string emotionName)
        {
            var aiResult = await _aiService.AnalyzeJournalAsync(entry.Content, emotionName);
            
            entry.AiFeedback = aiResult.Feedback;
            entry.AiPattern = aiResult.Pattern;
            
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;

            return await _repository.AddAsync(entry);
        }
    }
}