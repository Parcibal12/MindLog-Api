using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Core.Domain.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MindLog.Api.Infrastructure.Services
{
    public class QuestPdfReportGenerator : IReportGenerator
    {
        public QuestPdfReportGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateClinicalPdfAsync(ClinicalSummary summary)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(x => ComposeHeader(x, summary));
                    page.Content().Element(x => ComposeContent(x, summary));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return await Task.FromResult(document.GeneratePdf());
        }

        private void ComposeHeader(IContainer container, ClinicalSummary summary)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("MINDLOG - REPORTE CLÍNICO").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Paciente: {summary.PatientName}");
                    column.Item().Text($"Periodo: {summary.PeriodStart:dd/MM/yyyy} - {summary.PeriodEnd:dd/MM/yyyy}");
                });
            });
        }

        private void ComposeContent(IContainer container, ClinicalSummary summary)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().Text("Resumen Estadístico").FontSize(14).SemiBold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Total de entradas registradas:");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(summary.TotalEntriesRecorded.ToString());

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Emoción predominante:");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(summary.DominantEmotion);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Patrón cognitivo principal:");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(summary.DominantCognitivePattern);
                });

                column.Item().Text("Entradas de Alta Intensidad o Riesgo").FontSize(14).SemiBold();
                
                if (!summary.CriticalEntries.Any())
                {
                    column.Item().Text("No se detectaron entradas críticas en este periodo.").Italic();
                }
                else
                {
                    foreach (var entry in summary.CriticalEntries)
                    {
                        column.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text($"{entry.Date:dd/MM/yyyy} - Intensidad: {entry.Intensity}/10").SemiBold();
                            c.Item().Text($"Emoción: {entry.Emotion} | Patrón: {entry.AiPattern}").FontColor(Colors.Grey.Darken2);
                            c.Item().PaddingTop(5).Text($"\"{entry.Content}\"").Italic();
                        });
                    }
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        }
    }
}