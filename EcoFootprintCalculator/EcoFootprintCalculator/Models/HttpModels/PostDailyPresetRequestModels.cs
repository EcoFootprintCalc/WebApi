namespace EcoFootprintCalculator.Models.HttpModels
{
    public class PostDailyPresetRequest
    {
        public List<DailyPreset> Presets { get; set; } = new();
    }

    public class PostDailyPresetAIRequest
    {
        public string DayDescription { get; set; } = null!;
    }

    public class DailyPreset
    {
        public int PresetId { get; set; } = 0;
        public int Count { get; set; } = 0;
    }
}
