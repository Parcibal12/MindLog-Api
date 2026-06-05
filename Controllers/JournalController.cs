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
        private readonly ILogger<JournalController> _logger;

        public JournalController(
            IJournalService journalService, 
            IJournalEntryRepository repository, 
            ILogger<JournalController> logger)
        {
            _journalService = journalService;
            _repository = repository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] CreateJournalDto request)
        {
            try
            {                
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear la entrada del diario para el usuario {UserId}", request.UserId);
                return StatusCode(500, new { message = "Ocurrió un error interno en el servidor al procesar tu solicitud. Intenta nuevamente." });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserEntries(Guid userId)
        {
            try
            {
                var entries = await _repository.GetAllByUserIdAsync(userId);
                
                if (!entries.Any())
                    return NotFound(new { message = "No se encontraron diarios para este usuario." });

                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al consultar los diarios del usuario {UserId}", userId);
                return StatusCode(500, new { message = "Ocurrió un error al cargar tus datos emocionales." });
            }
        }
    }
}