namespace EcoFootprintCalculator.Services
{
    public interface IGeminiService
    {
        Task<int?> GetCustomActivityFootprintAsync(string activityDescription);
    }
}
