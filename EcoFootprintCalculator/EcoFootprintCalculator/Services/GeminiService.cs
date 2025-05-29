using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace EcoFootprintCalculator.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey");
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
            new { parts = new[] { new { text = combinedPrompt } } }
        },
                generationConfig = new
                {
                    maxOutputTokens = 200      // Itt lehet állítani a MAX választ!
                                               // temperature = 0.7,      // Opcionálisan: kreativitás mértéke (alap 1, max 2)
                                               // topK = 32,              // Opcionális: N válaszból a legvalószínûbbek közül válasszon
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

