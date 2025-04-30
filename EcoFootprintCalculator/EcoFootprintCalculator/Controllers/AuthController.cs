using EcoFootprintCalculator.Models.HttpModels;
using EcoFootprintCalculator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoFootprintCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            //Until db is not connected
            if (request.Username == "user" && request.Password == "1234")
            {
                var token = _tokenService.GenerateToken(request.Username);
                return Ok(new { token });
            }

            return Unauthorized();
        }

        [Authorize]
        public IActionResult LoginCheck()
        {
            var username = User.Identity?.Name;
            return Ok($"Logged in as {username}.");
        }
    }
}
