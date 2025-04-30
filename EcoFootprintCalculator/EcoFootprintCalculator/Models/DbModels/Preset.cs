using System;
using System.Collections.Generic;

namespace EcoFootprintCalculator.Models.DbModels;

public partial class Preset
{
    public int ID { get; set; }

    public string Description { get; set; } = null!;

    public int? Unit { get; set; }

    public int? Multiplier { get; set; }

    public int? CategoryID { get; set; }
}
