using EcoFootprintCalculator.Lib;
using EcoFootprintCalculator.Models.DbModels;
using EcoFootprintCalculator.Models.HttpModels;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("PostDailyPreset")]
        public IActionResult PostDailyPreset([FromBody] PostDailyPresetRequest DailyPreset)
        {
            //Until db is not connected
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            Random r = new();
            return Ok(new { CalculatedCo2 = r.Next(20 * DailyPreset.Presets.Count, 100 * DailyPreset.Presets.Count) });
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

        [HttpGet("GetSummarizedDailyFootprint")]
        public IActionResult GetSummarizedDailyFootprint()
        {
            //Until db is not connected
            Random r = new();
            return Ok(new { CalculatedCo2 = r.Next(1, 1000000)});
        }
    }
}
