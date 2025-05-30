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

        public async Task<string> GenerateTextAsync(string prompt)
        {
            // (1) System prompt
            var systemPrompt = "You are an eco-friendly assistant helping users reduce their carbon footprint. Focus on practical, actionable advice for sustainable living, energy conservation, transportation alternatives, waste reduction, and environmentally conscious choices. Keep responses helpful, encouraging, and factual. Be short and don't use markdown.";
            var combinedPrompt = systemPrompt + "\n\n" + prompt;

            // (2) Request body + max válasz hossz
            var requestBody = new
            {
                contents = new[]
                {
                    new 
                    { 
                        parts = new[]
                        { 
                            new { text = combinedPrompt } 
                        } 
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 200      // Itt lehet �ll�tani a MAX v�laszt!
                                               // temperature = 0.7,      // Opcion�lisan: kreativit�s m�rt�ke (alap 1, max 2)
                                               // topK = 32,              // Opcion�lis: N v�laszb�l a legval�sz�n�bbek k�z�l v�lasszon
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}")
            {
                Content = JsonContent.Create(requestBody)
            };

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API error ({response.StatusCode}): {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }
    }
}
