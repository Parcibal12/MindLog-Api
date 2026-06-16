using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MindLog.Api.Core.Domain.Interfaces;

namespace MindLog.Api.Infrastructure.BackgroundJobs
{
    public class WeeklyReportJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public WeeklyReportJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                // Verificamos si es domingo
                if (now.DayOfWeek == DayOfWeek.Sunday)
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var reportService = scope.ServiceProvider.GetRequiredService<IClinicalReportService>();
                            
                            Console.WriteLine($"[{DateTime.UtcNow}] Iniciando envío automático dominical...");
                            await reportService.ProcessAutomaticReportsAsync();
                            Console.WriteLine($"[{DateTime.UtcNow}] Envío automático finalizado con éxito.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en el trabajo de fondo: {ex.Message}");
                    }

                    await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}