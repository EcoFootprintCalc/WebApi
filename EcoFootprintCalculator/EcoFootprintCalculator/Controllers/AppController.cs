using EcoFootprintCalculator.Lib;
using EcoFootprintCalculator.Models.DbModels;
using EcoFootprintCalculator.Models.HttpModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoFootprintCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppController : BaseController
    {
        public AppController(MySQL _mysql) : base(_mysql) { }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetPresets")]
        public IActionResult GetPresets()
        {
            return Ok(_mysql.Presets.ToList());
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
            return Ok( new {Success = true, CurrentFootprint = summarized, SummarizedDailyFootprint = dailySummarized } );
        }

        [HttpPost("PostDailyPresetAI")]
        public IActionResult PostDailyPresetAI([FromBody] PostDailyPresetAIRequest DayDescription)
        {
            //Until db is not connected
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            Random r = new();
            return Ok(new { CalculatedCo2 = r.Next(DayDescription.DayDescription.Length * 10, DayDescription.DayDescription.Length * 1000) });
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
            
            return Ok(new {Success = true, DailyFootprintAmount = responseList });
        }
    }
}
