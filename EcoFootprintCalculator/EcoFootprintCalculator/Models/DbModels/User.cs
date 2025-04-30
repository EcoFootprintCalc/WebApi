using System;
using System.Collections.Generic;

namespace EcoFootprintCalculator.Models.DbModels;

public partial class User
{
    public int ID { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Pwd { get; set; } = null!;

    public int? ProfileIMG { get; set; }

    public DateTime? LastLoginDate { get; set; }
}
