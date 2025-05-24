namespace EcoFootprintCalculator.Models.HttpModels
{
    public class GetMonthlyFootprintRequest
    {
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
