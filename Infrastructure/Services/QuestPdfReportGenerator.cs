using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Core.Domain.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace MindLog.Api.Infrastructure.Services
{
    public class QuestPdfReportGenerator : IReportGenerator
    {
        private readonly string MindLogGreen = "#14B8A6"; 
        private readonly string DarkGreyText = "#333333";
        private readonly string LightGreyDivider = "#EAEAEA";

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
                    page.Margin(2.5f, Unit.Centimetre); 
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(DarkGreyText));

                    page.Header().Element(x => ComposeHeader(x, summary));
                    page.Content().Element(x => ComposeContent(x, summary));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return await Task.FromResult(document.GeneratePdf());
        }

        private void ComposeHeader(IContainer container, ClinicalSummary summary)
        {
            container.PaddingBottom(20).BorderBottom(1).BorderColor(LightGreyDivider).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("MINDLOG").FontSize(24).Black().FontColor(MindLogGreen);
                    column.Item().Text("REPORTE CLÍNICO").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                    
                    column.Item().PaddingTop(10).Text(text => 
                    {
                        text.Span("Paciente: ").SemiBold();
                        text.Span(summary.PatientName);
                    });
                    column.Item().Text(text => 
                    {
                        text.Span("Periodo: ").SemiBold();
                        text.Span($"{summary.PeriodStart:dd/MM/yyyy} - {summary.PeriodEnd:dd/MM/yyyy}");
                    });
                });

                try
                {
                    byte[] logoBytes = Convert.FromBase64String(ReportAssets.MindLogLogoBase64);
                    row.ConstantItem(80).Image(logoBytes);
                }
                catch (FormatException)
                {
                    row.ConstantItem(80).Text("MINDLOG").FontColor(MindLogGreen).SemiBold();
                }
            });
        }

        private void ComposeContent(IContainer container, ClinicalSummary summary)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(25);

                column.Item().Text("Resumen Estadístico").FontSize(14).SemiBold().FontColor(MindLogGreen);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    DrawTableCell(table, "Total de entradas registradas:", summary.TotalEntriesRecorded.ToString());
                    DrawTableCell(table, "Emoción predominante:", summary.DominantEmotion);
                    DrawTableCell(table, "Patrón cognitivo principal:", summary.DominantCognitivePattern);
                });

                column.Item().Text("Entradas Recientes").FontSize(14).SemiBold().FontColor(MindLogGreen);
                
                if (!summary.CriticalEntries.Any())
                {
                    column.Item().Text("No se detectaron entradas en este periodo.").Italic().FontColor(Colors.Grey.Medium);
                }
                else
                {
                    foreach (var entry in summary.CriticalEntries)
                    {
                        column.Item().Background("#F9FAFB").BorderLeft(3).BorderColor(MindLogGreen).Padding(10).Column(c =>
                        {
                            c.Item().Row(r => 
                            {
                                r.RelativeItem().Text($"{entry.Date:dd/MM/yyyy}").SemiBold();
                                r.ConstantItem(100).AlignRight().Text($"Intensidad: {entry.Intensity}/10").SemiBold().FontColor(MindLogGreen);
                            });
                            c.Item().Text($"Emoción: {entry.Emotion} | Patrón: {entry.AiPattern}").FontSize(9).FontColor(Colors.Grey.Medium);
                            c.Item().PaddingTop(5).Text($"\"{entry.Content}\"").Italic();
                        });
                    }
                }
            });
        }

        private void DrawTableCell(TableDescriptor table, string label, string value)
        {
            table.Cell().BorderBottom(1).BorderColor(LightGreyDivider).PaddingVertical(5).Text(label).SemiBold();
            table.Cell().BorderBottom(1).BorderColor(LightGreyDivider).PaddingVertical(5).Text(value).AlignRight();
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(LightGreyDivider).PaddingTop(10).Column(column => 
            {
                column.Item().AlignCenter().Text("MindLog © 2026 | Análisis generado por IA").FontSize(8).FontColor(Colors.Grey.Medium);
                column.Item().AlignCenter().Text("Este documento es estrictamente confidencial. No reemplaza el criterio médico o psicológico profesional.").FontSize(8).FontColor(Colors.Grey.Medium);
                
                column.Item().AlignCenter().PaddingTop(5).Text(x =>
                {
                    x.Span("Página ").FontSize(8).FontColor(Colors.Grey.Lighten1);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Lighten1);
                    x.Span(" de ").FontSize(8).FontColor(Colors.Grey.Lighten1);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Lighten1);
                });
            });
        }
    }
}