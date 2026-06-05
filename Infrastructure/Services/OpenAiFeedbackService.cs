using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MindLog.Api.Core.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MindLog.Api.Infrastructure.Services
{
    public class OpenAiFeedbackService : IAiFeedbackService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAiFeedbackService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<(string Feedback, string Pattern)> AnalyzeJournalAsync(string content, string emotionName)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY")
            {
                return ("Gracias por registrar tus emociones hoy. Sigue así.", "NEUTRAL");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var prompt = $@"
Eres un asistente terapéutico cognitivo-conductual empático. 
Analiza este diario de un paciente cuya emoción principal hoy es '{emotionName}'.
Diario: '{content}'

Devuelve ESTRICTAMENTE un objeto JSON con dos propiedades:
1. 'feedback': Un mensaje corto, humano y empático (máximo 3 líneas) haciendo de 'espejo cognitivo'.
2. 'pattern': Una etiqueta en MAYÚSCULAS del patrón cognitivo detectado (ej. CATASTROFIZACION, PENSAMIENTO_ABSOLUTISTA, ANSIEDAD_ANTICIPATORIA, NEUTRAL).
No agregues texto extra, solo el JSON.";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseString);
                
                var aiResponseContent = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiResult = JsonSerializer.Deserialize<AiResponseFormat>(aiResponseContent!, options);
                
                return (aiResult?.Feedback ?? "Reflexión generada.", aiResult?.Pattern ?? "NO_DETECTADO");
            }
            catch (Exception)
            {
                return ("Hemos guardado tu entrada exitosamente.", "ERROR_IA");
            }
        }

        private class AiResponseFormat
        {
            public string? Feedback { get; set; }
            public string? Pattern { get; set; }
        }
    }
}