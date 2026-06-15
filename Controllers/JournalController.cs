using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindLog.Api.Core.Application.DTOs;
using MindLog.Api.Core.Application.Services;
using MindLog.Api.Core.Domain.Entities;
using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Infrastructure.Data; 

namespace MindLog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalController : ControllerBase
    {
        private readonly IJournalService _journalService;
        private readonly IJournalEntryRepository _repository;
        private readonly ILogger<JournalController> _logger;
        private readonly IAiFeedbackService _aiFeedbackService;
        private readonly MindLogDbContext _context;

        public JournalController(
            IJournalService journalService, 
            IJournalEntryRepository repository, 
            ILogger<JournalController> logger,
            IAiFeedbackService aiFeedbackService,
            MindLogDbContext context)
        {
            _journalService = journalService;
            _repository = repository;
            _logger = logger;
            _aiFeedbackService = aiFeedbackService;
            _context = context;
        }

        public class AnalyzeRequest { public required string Content { get; set; } }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeEntry([FromBody] AnalyzeRequest request)
        {
            try
            {
                var (feedback, pattern) = await _aiFeedbackService.AnalyzeJournalAsync(request.Content, "No especificada");
                return Ok(new { feedback, pattern });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar con IA");
                return StatusCode(500, new { message = "Error al procesar la IA." });
            }
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
                    AiFeedback = request.AiFeedback, 
                    AiPattern = request.AiPattern,   
                    EntryContexts = request.ContextTagIds.Select(tagId => new EntryContext { TagId = tagId }).ToList()
                };

                var createdEntry = await _journalService.CreateJournalEntryAsync(entry, request.EmotionName);
                return Ok(createdEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear la entrada del diario");
                return StatusCode(500, new { message = "Ocurrió un error interno en el servidor." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEntries()
        {
            try
            {
                var userId = Guid.Parse("648bea7c-175d-4caa-8c3b-1ea519b93e46");
                var entries = await _repository.GetAllByUserIdAsync(userId);
                
                if (entries == null || !entries.Any())
                    return Ok(new List<object>());

                var response = entries.Select(e => new
                {
                    userId = e.UserId.ToString(),
                    content = e.Content ?? string.Empty,
                    emotionId = e.EmotionId,
                    emotionName = e.Emotion?.Name ?? "Desconocido", 
                    intensity = e.Intensity,
                    contextTagIds = e.EntryContexts?.Select(ec => ec.TagId).ToList() ?? new List<int>(),
                    createdAt = e.CreatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al consultar los diarios generales");
                return StatusCode(500, new { message = "Ocurrió un error al cargar el historial." });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserEntries(Guid userId)
        {
            try
            {
                var entries = await _repository.GetAllByUserIdAsync(userId);
                if (!entries.Any()) return NotFound(new { message = "No se encontraron diarios para este usuario." });
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico");
                return StatusCode(500, new { message = "Ocurrió un error." });
            }
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var userId = Guid.Parse("648bea7c-175d-4caa-8c3b-1ea519b93e46");
                var startDate = DateTime.UtcNow.AddDays(-30);
                var endDate = DateTime.UtcNow;
                
                var stats = await _repository.GetAnalyticsSummaryAsync(userId, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al consultar analíticas");
                return StatusCode(500, new { message = "Error al cargar las estadísticas." });
            }
        }

        [HttpGet("emotions")]
        public async Task<IActionResult> GetEmotions()
        {
            var emotions = await _context.Emotions
                .AsNoTracking()
                .Select(e => new { e.Id, e.Name, e.ColorHex })
                .ToListAsync();
            return Ok(emotions);
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetContextTags()
        {
            var tags = await _context.ContextTags
                .AsNoTracking()
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return Ok(tags);
        }
    }
}