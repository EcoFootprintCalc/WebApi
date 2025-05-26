using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using EcoFootprintCalculator.Models.AppModels;

namespace EcoFootprintCalculator.Services
{
    public class GeminiService: IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _prompt;

        public GeminiService(HttpClient httpClient, IOptions<GeminiApiSettings> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _prompt = options.Value.Prompt;
        }

        public async Task<int?> GetCustomActivityFootprintAsync(string activityDescription)
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
                            new { text = _prompt.Replace("{ActivityDescription}", activityDescription) }
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
            var text = doc.RootElement
                          .GetProperty("candidates")[0]
                          .GetProperty("content")
                          .GetProperty("parts")[0]
                          .GetProperty("text")
                          .GetString();
            return int.TryParse(text, out int result) ? result : null;
        }
    }
}
