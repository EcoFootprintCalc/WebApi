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

        private List<Preset> tempPresets = new() { 
            new Preset() { ID = 1, CategoryID = 1, Description = "Desc1", Multiplier = 10, Unit = "l" },
            new Preset() { ID = 2, CategoryID = 1, Description = "Desc2", Multiplier = 2, Unit = "kg" },
            new Preset() { ID = 3, CategoryID = 1, Description = "Desc3", Multiplier = 0.5, Unit = "m^3" },
            new Preset() { ID = 4, CategoryID = 2, Description = "Desc4", Multiplier = 10000, Unit = "kg" },
            new Preset() { ID = 5, CategoryID = 3, Description = "Desc5", Multiplier = 10, Unit = "km" }
        };

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetPresets")]
        public IActionResult GetPresets()
        {
            //Until db is not connected
            return Ok(tempPresets);
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
