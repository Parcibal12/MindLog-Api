using Microsoft.AspNetCore.Mvc;
using MindLog.Api.Core.Application.Services.DTOs;
using MindLog.Api.Core.Domain.Interfaces;

namespace MindLog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IJournalEntryRepository _repository;

        public AnalyticsController(IJournalEntryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{userId:guid}/summary")]
        public async Task<ActionResult<AnalyticsSummaryDto>> GetWeeklySummary(
            [FromRoute] Guid userId, 
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate)
        {

            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-7);

            try
            {
                var summary = await _repository.GetAnalyticsSummaryAsync(userId, start, end);
                
                return Ok(summary); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    message = "Ocurrió un error interno al calcular las métricas analíticas.", 
                    detail = ex.Message 
                });
            }
        }
    }
}