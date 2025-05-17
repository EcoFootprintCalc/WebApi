using System;
using System.Collections.Generic;

namespace EcoFootprintCalculator.Models.DbModels;

public partial class Preset
{
    public int ID { get; set; }

    public string Description { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public double Multiplier { get; set; }

    public int CategoryID { get; set; }
}
