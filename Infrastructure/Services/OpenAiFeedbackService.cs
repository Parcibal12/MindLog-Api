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

            var promptTemplate = _configuration["AiSettings:PromptTemplate"];
            var prompt = string.Format(promptTemplate!, emotionName, content);

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            try
            {
                var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n==================================");
                    Console.WriteLine($"ERROR DE API EXTERNA (Código {response.StatusCode}):");
                    Console.WriteLine(errorDetails);
                    Console.WriteLine("==================================\n");
                    return ("Hemos guardado tu entrada exitosamente.", "ERROR_IA");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseString);
                
                var aiResponseContent = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (!string.IsNullOrEmpty(aiResponseContent))
                {
                    aiResponseContent = aiResponseContent
                        .Replace("```json", "")
                        .Replace("```", "")
                        .Trim();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiResult = JsonSerializer.Deserialize<AiResponseFormat>(aiResponseContent!, options);
                
                return (aiResult?.Feedback ?? "Reflexión generada.", aiResult?.Pattern ?? "NO_DETECTADO");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n==================================");
                Console.WriteLine($"ERROR INTERNO: {ex.Message}");
                Console.WriteLine("==================================\n");
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