namespace EcoFootprintCalculator.Models.HttpModels
{
    public class TravelRequest
    {
        public int Persons { get; set; } = 1;
        public int Distance { get; set; } = 0;
        public int? CarId { get; set; } = null;
    }
}
