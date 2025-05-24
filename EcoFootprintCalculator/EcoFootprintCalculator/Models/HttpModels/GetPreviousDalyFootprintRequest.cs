namespace EcoFootprintCalculator.Models.HttpModels
{
    public class GetPreviousDalyFootprintRequest
    {
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
