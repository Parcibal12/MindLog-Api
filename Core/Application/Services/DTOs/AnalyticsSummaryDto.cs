using System;
using System.Collections.Generic;

namespace MindLog.Api.Core.Application.Services.DTOs
{
    public class AnalyticsSummaryDto
    {
        public int TotalEntries { get; set; }
        public string DominantEmotion { get; set; } = "Ninguna";
        public string DominantPattern { get; set; } = "Ninguno";

        public Dictionary<string, int> EmotionDistribution { get; set; } = new();

        public List<ContextTagCountDto> TopDisparadores { get; set; } = new();

        public List<DailyIntensityDto> MoodTrend { get; set; } = new();
    }

    public class ContextTagCountDto
    {
        public string TagName { get; set; } = string.Empty;
        public int Count { get; set; }
        public string DominantEmotion { get; set; } = "Ninguna";
    }

    public class DailyIntensityDto
    {
        public DateTime Date { get; set; }
        public double AverageIntensity { get; set; }
    }
}