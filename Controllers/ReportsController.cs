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
                
                return Ok(new { message = "Proceso de reportes finalizado. El correo ha sido enviado al terapeuta correspondiente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n=== ERROR FATAL AL ENVIAR REPORTE ===");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=====================================\n");

                return StatusCode(500, new 
                { 
                    error = "Ocurrió un error al procesar los reportes clínicos.", 
                    details = ex.Message 
                });
            }
        }
    }
}