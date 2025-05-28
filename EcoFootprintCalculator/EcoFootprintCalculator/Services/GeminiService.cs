using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using EcoFootprintCalculator.Models.AppModels;
using static System.Net.Mime.MediaTypeNames;

namespace EcoFootprintCalculator.Services
{
    public class GeminiService: IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _prompt;
        private readonly string _recommendationPrompt;

        public GeminiService(HttpClient httpClient, IOptions<GeminiApiSettings> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _prompt = options.Value.Prompt;
            _recommendationPrompt = options.Value.RecommendationPrompt;
        }

        public async Task<string?> GetCustomActivityFootprintAsync(string activityDescription, string categories)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = _prompt.Replace("{ActivityDescription}", activityDescription).Replace("{Categories}", categories) }
                        }
                    }
                }
            };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
                return null;
            var resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);
            return doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? null;
        }

        //Travel: 6000, Shopping: 3500, Freetime activities: 800, Home: 1200
        public async Task<string?> GetPersonalRecommendationAsync(string dailyString)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = _recommendationPrompt.Replace("{DailyFootprintCategories}", dailyString) }
                        }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = new
                    {
                        type = "OBJECT",
                        properties = new {
                            shortRecommendation = new { type = "STRING" },
                            detailedRecommendation = new { type = "STRING" }
                        },
                        propertyOrdering = new[] { "shortRecommendation", "detailedRecommendation" }
                    }
                }
            };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
                return null;
            var resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);
            return doc.RootElement
                          .GetProperty("candidates")[0]
                          .GetProperty("content")
                          .GetProperty("parts")[0]
                          .GetProperty("text")
                          .GetString();
        }
    }
}
