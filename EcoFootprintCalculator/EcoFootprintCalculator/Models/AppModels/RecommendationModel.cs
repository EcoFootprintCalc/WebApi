namespace EcoFootprintCalculator.Models.AppModels
{
    public class RecommendationModel
    {
        public int userId { get; set; }
        public DateTime createdAt {  get; set; }
        public string shortRecommendation {  get; set; } = string.Empty;
        public string detailedRecommendation { get; set; } = string.Empty;
    }
}
