using System;
using System.Collections.Generic;

namespace EcoFootprintCalculator.Models.DbModels;

public partial class Car
{
    public int ID { get; set; }

    public string? Name { get; set; }

    public string Brand { get; set; } = null!;

    public string Type { get; set; } = null!;

    public float AvgFuelConsumption { get; set; }

    public int? UserID { get; set; }

    public int? CategoryID { get; set; }
}
