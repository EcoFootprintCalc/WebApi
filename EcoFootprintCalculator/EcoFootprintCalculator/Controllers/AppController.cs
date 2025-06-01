using EcoFootprintCalculator.Lib;
using EcoFootprintCalculator.Models.AppModels;
using EcoFootprintCalculator.Models.DbModels;
using EcoFootprintCalculator.Models.HttpModels;
using EcoFootprintCalculator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcoFootprintCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppController : BaseController
    {
        public AppController(MySQL mysql, IGeminiService geminiService) : base(mysql, geminiService) { }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetPresets")]
        public IActionResult GetPresets()
        {
            return Ok(_mysql.Presets.ToList());
        }

        [HttpGet("GetCategories")]
        public IActionResult GetCategories()
        {
            return Ok(_mysql.Categories.ToList());
        }

        [Authorize]
        [HttpPost("PostDailyPreset")]
        public async Task<IActionResult> PostDailyPreset([FromBody] DailyPresetRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            Preset p = await _mysql.Presets.SingleAsync(p => p.ID == request.PresetId);
            double summarized = p.Multiplier * request.Count;

            Footprint? fp;
            if ((fp = await _mysql.Footprints.SingleOrDefaultAsync(f=>f.UserID == logonId && f.CategoryID == p.CategoryID && f.Date.Date == DateTime.Now.Date)) != null)
            {
                fp.CarbonFootprintAmount += summarized;
                _mysql.Footprints.Update(fp);
            }
            else
                await _mysql.Footprints.AddAsync(new Footprint{ CategoryID = p.CategoryID, Date = DateTime.Now.Date, UserID = logonId, CarbonFootprintAmount = summarized });
            await _mysql.SaveChangesAsync();

            double dailySummarized = _mysql.Footprints.Where(fp => fp.UserID == logonId && fp.Date.Date == DateTime.Now.Date).Sum(fp => fp.CarbonFootprintAmount);
            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date == DateTime.Now.Date).ToList().ForEach(t => dailySummarized += Math.Round(t.Distance_km/100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));

            return Ok( new {Success = true, CurrentFootprint = summarized, SummarizedDailyFootprint = dailySummarized } );
        }

        [Authorize]
        [HttpPost("PostAIActivity")]
        public async Task<IActionResult> PostAIActivity([FromBody] PostAIActivityRequest request)
        {
            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            string categoriesString = string.Join(", ",
                _mysql.Categories.ToList().Select(c =>
                {
                    return $"{c.Description}: {c.ID}";
                }));

            string? result = await _geminiService!.GetCustomActivityFootprintAsync(request.ActivityDescription, categoriesString);

            if (result == null)
                return BadRequest(new { Success = false, Msg = "The eco footprint of the activity could not be determined!" });

            int category = 5;
            int value = 0;
            if (result.Split(" ").Length == 2)
            {
                value = Convert.ToInt32(result.Split(" ")[0]);
                if (int.TryParse(result.Split(" ")[1], out int parsed))
                    category = parsed;
            }
            else if(result.Split(" ").Length == 1)
            {
                value = Convert.ToInt32(result.Split(" ")[0]);
            }
            else {
                return BadRequest(new { Success = false, Msg = "The eco footprint of the activity could not be determined!" });
            }

            Footprint? fp;
            if ((fp = await _mysql.Footprints.SingleOrDefaultAsync(f => f.UserID == logonId && f.CategoryID == category && f.Date.Date == DateTime.Now.Date)) != null) // 5 - Other
            {
                fp.CarbonFootprintAmount += (double)value;
                _mysql.Footprints.Update(fp);
            }
            else
                await _mysql.Footprints.AddAsync(new Footprint { CategoryID = (int)category, Date = DateTime.Now.Date, UserID = logonId, CarbonFootprintAmount = (double)value }); // 5 - Other
            await _mysql.SaveChangesAsync();

            double dailySummarized = _mysql.Footprints.Where(fp => fp.UserID == logonId && fp.Date.Date == DateTime.Now.Date).Sum(fp => fp.CarbonFootprintAmount);
            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date == DateTime.Now.Date).ToList().ForEach(t => dailySummarized += Math.Round(t.Distance_km / 100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));

            return Ok(new {Success = true, CurrentActivityFootprint = value, CurrentActivityCategory = category, SummarizedDailyFootprint = dailySummarized });
        }

        [Authorize]
        [HttpGet("GetSummarizedDailyFootprint")]
        public async Task<IActionResult> GetSummarizedDailyFootprint()
        {
            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            List<DailyFootprintResponse> responseList = new();
            _mysql.Footprints.Where(fp=>fp.UserID == logonId && fp.Date.Date == DateTime.Today.Date).ToList().ForEach(x=> responseList.Add(new DailyFootprintResponse() { CategoryID = x.CategoryID, FootprintAmount = x.CarbonFootprintAmount }));

            int DailyTravelFootprint = 0;
            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date == DateTime.Today.Date).ToList().ForEach(t => DailyTravelFootprint += (int)Math.Round(t.Distance_km/100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));

            if(DailyTravelFootprint > 0)
            {
                if (responseList.Any(rl => rl.CategoryID == 1))
                    responseList.Single(rl => rl.CategoryID == 1).FootprintAmount += DailyTravelFootprint;
                else
                    responseList.Add(new DailyFootprintResponse() { CategoryID = 1, FootprintAmount = DailyTravelFootprint });
            }

            return Ok(new {Success = true, DailyFootprintAmount = responseList });
        }

        [Authorize]
        [HttpGet("GetOverTimeFootprint")]
        public async Task<IActionResult> GetOverTimeFootprint()
        {
            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            DateTime endTime = DateTime.Today;
            DateTime startTime = DateTime.Today.AddDays(-30);

            List<OverTimeFootprintResponse> response = new();
            List<Footprint> footprints = _mysql.Footprints.Where(fp => fp.UserID == logonId && fp.Date.Date >= startTime.Date && fp.Date.Date <= endTime.Date).ToList();
            List<Travel> travels = _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date >= startTime && t.Date.Date <= endTime).ToList();


            foreach (DateTime date in Statics.GetDatesBetween(startTime, endTime))
            {
                int DailyTravelFootprint = 0;
                travels.Where(t => t.Date.Date == date.Date).ToList().
                    ForEach(t => DailyTravelFootprint += (int)Math.Round(t.Distance_km / 100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));

                int DailyFootprint = 0;
                footprints.Where(fp => fp.Date == date).ToList().ForEach(x => DailyFootprint += (int)Math.Round(x.CarbonFootprintAmount, 0));

                response.Add(new OverTimeFootprintResponse {
                    Date = date,
                    FootprintAmount = DailyTravelFootprint + DailyFootprint
                });
            }

            return Ok(new { Success = true, FootprintList = response });
        }

        [Authorize]
        [HttpGet("GetPreviousDailyFootprint")]
        public async Task<IActionResult> GetPreviousDailyFootprint(DateTime Date)
        {
            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            List<DailyFootprintResponse> responseList = new();
            _mysql.Footprints.Where(fp => fp.UserID == logonId && fp.Date.Date == Date.Date).ToList().ForEach(x => responseList.Add(new DailyFootprintResponse() { CategoryID = x.CategoryID, FootprintAmount = x.CarbonFootprintAmount }));


            int DailyTravelFootprint = 0;
            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date == Date.Date).ToList().ForEach(t => DailyTravelFootprint += (int)Math.Round(t.Distance_km/100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));

            if (DailyTravelFootprint > 0)
            {
                if (responseList.Any(rl => rl.CategoryID == 1))
                    responseList.Single(rl => rl.CategoryID == 1).FootprintAmount += DailyTravelFootprint;
                else
                    responseList.Add(new DailyFootprintResponse() { CategoryID = 1, FootprintAmount = DailyTravelFootprint });
            }

            return Ok(new { Success = true, DailyFootprintAmount = responseList });
        }

        [Authorize]
        [HttpGet("GetMonthlyFootprint")]
        public async Task<IActionResult> GetMonthlyFootprint(DateTime Date)
        {
            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            List<DailyFootprintResponse> responseList = new();
            _mysql.Footprints.Where(fp => fp.UserID == logonId && fp.Date.Year == Date.Year && fp.Date.Month == Date.Month).GroupBy(f => f.CategoryID).ToList().ForEach(g => responseList.Add(new DailyFootprintResponse() { CategoryID = g.Key, FootprintAmount = g.Sum(f => f.CarbonFootprintAmount) }));

            int MonthlyTravelFootprint = 0;
            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Year == Date.Year && t.Date.Month == Date.Month).ToList().ForEach(t => MonthlyTravelFootprint += (int)Math.Round(t.Distance_km/100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));

            if (MonthlyTravelFootprint > 0)
            {
                if (responseList.Any(rl => rl.CategoryID == 1))
                    responseList.Single(rl => rl.CategoryID == 1).FootprintAmount += MonthlyTravelFootprint;
                else
                    responseList.Add(new DailyFootprintResponse() { CategoryID = 1, FootprintAmount = MonthlyTravelFootprint });
            }
            
            return Ok(new { Success = true, MonthlyFootprint = responseList });
        }

        [Authorize]
        [HttpGet("GetRecommendation")]
        public async Task<IActionResult> GetRecommendation(bool forceGeneration = false)
        {
            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            if(!_mysql.Footprints.Any(fp=>fp.UserID == logonId && fp.Date.Date == DateTime.Today.Date))
                return Ok(new { Success = false, Msg = "Personal recommendations are available only if you have at least one recorded footprint for today." });

            TempStorage.recommendations.RemoveAll(r=>r.createdAt.Date != DateTime.Today.Date);

            RecommendationModel rm;

            if (TempStorage.recommendations.Any(r => r.userId == logonId))
                if (TempStorage.recommendations.Single(r => r.userId == logonId).createdAt <= DateTime.UtcNow.AddMinutes(-60) || forceGeneration)
                { //Regenerate
                    rm = TempStorage.recommendations.Single(r => r.userId == logonId);
                    string requestString = string.Join(", ",
                    _mysql.Categories.ToList().Select(c =>
                    {
                        var amount = _mysql.Footprints.SingleOrDefault(fp => fp.UserID == logonId && fp.Date.Date == DateTime.Today.Date && fp.CategoryID == c.ID) ?.CarbonFootprintAmount ?? 0;
                        if (c.ID == 1)
                            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date == DateTime.Today.Date).ToList().ForEach(t => amount += (int)Math.Round(t.Distance_km / 100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));
                        return $"{c.Description}: {amount}";
                    }));
                    string? recommendation = await _geminiService!.GetPersonalRecommendationAsync(requestString);
                    if (recommendation is null)
                        return BadRequest(new { Success = false, Msg = "Cannot give recommendations" });
                    AIRecommendationResponseModel? arrm = JsonSerializer.Deserialize<AIRecommendationResponseModel>(recommendation!);
                    if(arrm is null)
                        return BadRequest(new { Success = false, Msg = "Cannot give recommendations" });

                    rm.shortRecommendation = arrm.shortRecommendation;
                    rm.detailedRecommendation = arrm.detailedRecommencdation;
                    rm.createdAt = DateTime.UtcNow;
                }
                else //Return current
                    rm = TempStorage.recommendations.Single(r => r.userId == logonId && r.createdAt.Date == DateTime.Today.Date);
            else
            { //Generate
                rm = new();
                string requestString = string.Join(", ",
                    _mysql.Categories.ToList().Select(c =>
                    {
                        var amount = _mysql.Footprints.SingleOrDefault(fp => fp.UserID == logonId && fp.Date.Date == DateTime.Today.Date && fp.CategoryID == c.ID)?.CarbonFootprintAmount ?? 0;
                        if (c.ID == 1)
                            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date == DateTime.Today.Date).ToList().ForEach(t => amount += (int)Math.Round(t.Distance_km / 100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));
                        return $"{c.Description}: {amount}";
                    }));
                string? recommendation = await _geminiService!.GetPersonalRecommendationAsync(requestString);
                if (recommendation is null)
                    return BadRequest(new { Success = false, Msg = "Cannot give recommendations" });
                AIRecommendationResponseModel? arrm = JsonSerializer.Deserialize<AIRecommendationResponseModel>(recommendation!);
                if (arrm is null)
                    return BadRequest(new { Success = false, Msg = "Cannot give recommendations" });

                rm.shortRecommendation = arrm.shortRecommendation;
                rm.detailedRecommendation = arrm.detailedRecommencdation;
                rm.userId = logonId;
                rm.createdAt = DateTime.UtcNow;
                TempStorage.recommendations.Add(rm);
            }

            return Ok( new { Success = true, ShortRecommendation = rm.shortRecommendation, DetailedRecommendation = rm.detailedRecommendation });
        }

        [Authorize]
        [HttpPost("PostDailyTravel")]
        public async Task<IActionResult> PostDailyTravel([FromBody] TravelRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" }); //Btw. its not possible?!

            Car? c = _mysql.Cars.SingleOrDefault(c => c.ID == request.CarId);
            if (c is null)
                return BadRequest(new {Success = false, Msg = "The given car does not exists."});

            if (c.UserID != logonId)
                return Unauthorized("User has no permission for the given car.");

            _mysql.Travels.Add(new Travel()
            {
                Persons = request.Persons,
                Distance_km = request.Distance,
                Date = DateTime.Now,
                UserID = logonId,
                CarID = request.CarId
            });

            await _mysql.SaveChangesAsync();

            int actualCost = (int)Math.Round(request.Distance/100.0 * _mysql.Cars.Single(c => c.ID == request.CarId).AvgFuelConsumption * Constants.FuelMultiplier / request.Persons * 1000, 0);

            double dailySummarized = _mysql.Footprints.Where(fp => fp.UserID == logonId && fp.Date.Date == DateTime.Now.Date).Sum(fp => fp.CarbonFootprintAmount);
            _mysql.Travels.Where(t => t.UserID == logonId && t.Date.Date == DateTime.Now.Date).ToList().ForEach(t => dailySummarized += Math.Round(t.Distance_km/100.0 * _mysql.Cars.Single(c => c.ID == t.CarID).AvgFuelConsumption * Constants.FuelMultiplier / t.Persons * 1000, 0));

            return Ok( new { Success = true, ActualTripCost = actualCost, SummarizedDailyCost = dailySummarized } );
        }
    }
}
