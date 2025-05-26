namespace EcoFootprintCalculator.Lib
{
    public class Statics
    {
        public static List<DateTime> GetDatesBetween(DateTime start, DateTime end)
        {
            List<DateTime> dates = new List<DateTime>();

            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            return dates;
        }
    }
}
