using Microsoft.AspNetCore.Mvc;
using MindLog.Api.Core.Application.DTOs;
using MindLog.Api.Core.Application.Services;
using MindLog.Api.Core.Domain.Entities;
using MindLog.Api.Core.Domain.Interfaces;

namespace MindLog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalController : ControllerBase
    {
        private readonly IJournalService _journalService;
        private readonly IJournalEntryRepository _repository;

        public JournalController(IJournalService journalService, IJournalEntryRepository repository)
        {
            _journalService = journalService;
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] CreateJournalDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "El contenido del diario no puede estar vacío." });

            if (request.Intensity < 1 || request.Intensity > 10)
                return BadRequest(new { message = "La intensidad debe estar entre 1 y 10." });

            var entry = new JournalEntry
            {
                UserId = request.UserId,
                Content = request.Content,
                EmotionId = request.EmotionId,
                Intensity = request.Intensity,
                EntryContexts = request.ContextTagIds.Select(tagId => new EntryContext { TagId = tagId }).ToList()
            };

            var createdEntry = await _journalService.CreateJournalEntryAsync(entry, request.EmotionName);

            return Ok(createdEntry);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserEntries(Guid userId)
        {
            var entries = await _repository.GetAllByUserIdAsync(userId);
            
            if (!entries.Any())
                return NotFound(new { message = "No se encontraron diarios para este usuario." });

            return Ok(entries);
        }
    }
}