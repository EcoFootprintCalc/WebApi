namespace EcoFootprintCalculator.Services
{
    public interface IGeminiService
    {
        Task<string?> GetCustomActivityFootprintAsync(string activityDescription, string categories);
        Task<string?> GetPersonalRecommendationAsync(string dailyString);
        Task<string> GenerateTextAsync(string prompt);
    }
}
