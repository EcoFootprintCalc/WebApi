﻿namespace EcoFootprintCalculator.Models.HttpModels
{
    public class PostAIActivityRequest
    {
        public string ActivityDescription { get; set; } = null!;
    }

    public class DailyPresetRequest
    {
        public int PresetId { get; set; } = 0;
        public int Count { get; set; } = 0;
    }
}
