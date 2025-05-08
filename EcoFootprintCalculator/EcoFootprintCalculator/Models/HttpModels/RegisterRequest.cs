namespace EcoFootprintCalculator.Models.HttpModels
{
    public class RegisterRequest
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty!;

        public string Pwd { get; set; } = string.Empty!;

        public int? ProfileIMG { get; set; } = -1;
    }
}
