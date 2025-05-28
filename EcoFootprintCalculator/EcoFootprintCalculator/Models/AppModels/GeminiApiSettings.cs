namespace EcoFootprintCalculator.Models.AppModels
{
    public class GeminiApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string RecommendationPrompt { get; set; } = string.Empty;
    }
}
