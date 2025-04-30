using System;
using System.Collections.Generic;

namespace EcoFootprintCalculator.Models.DbModels;

public partial class Category
{
    public int ID { get; set; }

    public string Description { get; set; } = null!;

    public string? Colour { get; set; }
}
