using System;
using System.Collections.Generic;

namespace EcoFootprintCalculator.Models.DbModels;

public partial class Footprint
{
    public int ID { get; set; }

    public double CarbonFootprintAmount { get; set; }

    public DateTime Date { get; set; }

    public int UserID { get; set; }

    public int CategoryID { get; set; }
}
