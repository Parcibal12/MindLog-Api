using Microsoft.AspNetCore.Mvc;
using MindLog.Api.Core.Domain.Interfaces;

namespace MindLog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IClinicalReportService _reportService;

        public ReportsController(IClinicalReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("trigger-automatic")]
        public async Task<IActionResult> TriggerAutomaticReports()
        {
            try
            {
                await _reportService.ProcessAutomaticReportsAsync();
                
                return Ok(new { message = "Proceso de reportes finalizado. Los correos han sido enviados a los terapeutas correspondientes." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = "Ocurrió un error al procesar los reportes clínicos.", 
                    details = ex.Message 
                });
            }
        }
    }
}