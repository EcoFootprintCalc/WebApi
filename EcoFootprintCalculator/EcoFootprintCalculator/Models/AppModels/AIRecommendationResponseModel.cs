using System.Text.Json.Serialization;

namespace EcoFootprintCalculator.Models.AppModels
{
    public class AIRecommendationResponseModel
    {
        [JsonPropertyName("shortRecommendation")]
        public string shortRecommendation {get;set;} = string.Empty;
        [JsonPropertyName("detailedRecommendation")]
        public string detailedRecommencdation {get;set;} = string.Empty;
    }
}
