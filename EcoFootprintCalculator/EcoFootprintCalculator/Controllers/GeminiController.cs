using EcoFootprintCalculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoFootprintCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public GeminiController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("GenerateText")]
        public async Task<IActionResult> GenerateText([FromBody] string prompt)
        {
            Console.WriteLine("KAPOTT PROMPT:");
            Console.WriteLine(prompt == null ? "NULL" : prompt);
            try
            {
                var result = await _geminiService.GenerateTextAsync(prompt);
                return Ok(new { Result = result });
            }
            catch (Exception ex)
            {
                // Hibát visszaadod
                return StatusCode(500, ex.Message);
            }
        }
    }
}