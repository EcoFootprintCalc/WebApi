namespace EcoFootprintCalculator.Models.HttpModels
{
    public class AddCarRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Brand {  get; set; } = string.Empty;
        public string Type {  get; set; } = string.Empty;
        public float AvgFuelConsumption { get; set; } = 0;
    }
}
