using EcoFootprintCalculator.Models.HttpModels;
using EcoFootprintCalculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoFootprintCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public GeminiController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("GenerateText")]
        public async Task<IActionResult> GenerateText([FromBody] ChatBotRequest request)
        {
            //Console.WriteLine("KAPOTT PROMPT:");
            //Console.WriteLine(prompt == null ? "NULL" : prompt);
            if (string.IsNullOrEmpty(request.prompt))
                return BadRequest(new { Success = false, Msg = "Prompt is null or empty."});
            try
            {
                var result = await _geminiService.GenerateTextAsync(request.prompt);
                return Ok(new { Success = true, Result = result });
            }
            catch (Exception ex)
            {
                // Hibát visszaadod
                return BadRequest(new { Success = false, Msg = ex.Message });
            }
        }
    }
}