using System;
using System.Collections.Generic;

namespace EcoFootprintCalculator.Models.DbModels;

public partial class Travel
{
    public int ID { get; set; }

    public int Persons { get; set; }

    public int Distance_km { get; set; }

    public DateTime Date { get; set; }

    public int? UserID { get; set; }

    public int? CarID { get; set; }
}
