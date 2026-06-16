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

        public async Task<AiAnalysisResult> AnalyzeJournalAsync(string content)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY" || apiKey == "API_KEY_EN_USER_SECRETS")
            {
                return GetFallbackResult("No se encontró la API Key configurada.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var promptTemplate = _configuration["AiSettings:PromptTemplate"];
            var finalPrompt = promptTemplate!.Replace("{0}", content);

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new { role = "user", content = finalPrompt }
                },
                temperature = 0.2,
                response_format = new { type = "json_object" } 
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            try
            {
                var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"\nERROR IA: {errorDetails}");
                    return GetFallbackResult("Error de conexión con el proveedor de IA.");
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
                    aiResponseContent = aiResponseContent.Replace("```json", "").Replace("```", "").Trim();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiResult = JsonSerializer.Deserialize<AiAnalysisResult>(aiResponseContent!, options);
                
                return aiResult ?? GetFallbackResult("La IA no devolvió un formato válido.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR INTERNO IA: {ex.Message}");
                return GetFallbackResult("Error interno procesando la Inteligencia Artificial.");
            }
        }

        private AiAnalysisResult GetFallbackResult(string debugMessage)
        {
            Console.WriteLine(debugMessage);
            return new AiAnalysisResult 
            { 
                Feedback = "Hemos guardado tu entrada, pero nuestro espejo cognitivo está descansando en este momento.", 
                Pattern = "NEUTRAL",
                EmotionId = 1,
                Intensity = 5,
                ContextTagIds = new List<int>()
            };
        }
    }
}