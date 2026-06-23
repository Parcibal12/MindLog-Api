using Microsoft.EntityFrameworkCore;
using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Core.Domain.Models;
using MindLog.Api.Infrastructure.Data;

namespace MindLog.Api.Core.Application.Services
{
    public class ClinicalReportService : IClinicalReportService
    {
        private readonly MindLogDbContext _dbContext;
        private readonly IReportGenerator _reportGenerator;
        private readonly IEmailSender _emailSender;

        public ClinicalReportService(
            MindLogDbContext dbContext,
            IReportGenerator reportGenerator,
            IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _reportGenerator = reportGenerator;
            _emailSender = emailSender;
        }

        public async Task ProcessAutomaticReportsAsync()
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-7);

            var eligibleUsers = await _dbContext.Users
                .Where(u => u.AutoSendReports == true && !string.IsNullOrEmpty(u.TherapistEmail))
                .ToListAsync();

            foreach (var user in eligibleUsers)
            {
                var userEntries = await _dbContext.JournalEntries
                    .AsNoTracking() 
                    .Include(j => j.Emotion) 
                    .Where(j => j.UserId == user.Id && j.CreatedAt >= startDate && j.CreatedAt <= endDate)
                    .ToListAsync();

                if (!userEntries.Any()) continue;

                var summary = new ClinicalSummary
                {
                    UserId = user.Id,
                    PatientName = user.FullName,
                    PsychologistEmail = user.TherapistEmail!,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TotalEntriesRecorded = userEntries.Count,
                    
                    DominantEmotion = userEntries
                        .GroupBy(e => e.Emotion?.Name ?? "Desconocida")
                        .OrderByDescending(g => g.Count())
                        .First().Key,

                    DominantCognitivePattern = userEntries
                        .Where(e => !string.IsNullOrEmpty(e.AiPattern) && e.AiPattern != "NEUTRAL")
                        .GroupBy(e => e.AiPattern)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "NINGUNO",

                    CriticalEntries = userEntries
                        .Select(e => new CriticalEntry
                        {
                            Date = e.CreatedAt,
                            Emotion = e.Emotion?.Name ?? "Desconocida",
                            Intensity = e.Intensity,
                            Content = e.Content,
                            AiPattern = e.AiPattern ?? "NO_DETECTADO"
                        }).ToList()
                };

                var pdfBytes = await _reportGenerator.GenerateClinicalPdfAsync(summary);

                var subject = $"MindLog: Reporte Clínico Semanal - {user.FullName}";
                var body = $"Estimado profesional,\n\nAdjunto encontrará el resumen clínico automatizado de su paciente {user.FullName} correspondiente a los últimos 7 días.\n\nAtentamente,\nEl equipo de MindLog.";
                var fileName = $"Reporte_{user.FullName.Replace(" ", "_")}_{DateTime.UtcNow:ddMMyyyy}.pdf";

                await _emailSender.SendEmailWithAttachmentAsync(user.TherapistEmail!, subject, body, pdfBytes, fileName);
            }
        }
    }
}