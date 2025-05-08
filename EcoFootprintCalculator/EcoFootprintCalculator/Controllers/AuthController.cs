using EcoFootprintCalculator.Lib;
using EcoFootprintCalculator.Models.DbModels;
using EcoFootprintCalculator.Models.HttpModels;
using EcoFootprintCalculator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoFootprintCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly MySQL _mysql;

        public AuthController(TokenService tokenService, MySQL MySql)
        {
            _tokenService = tokenService;
            _mysql = MySql;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!await _mysql.Users.AnyAsync(u => u.UserName == request.Username || u.Email == request.Username))
                return Unauthorized(new {Success = false, Msg = "User not found."});

            User user = await _mysql.Users.SingleAsync(u => u.UserName == request.Username || u.Email == request.Username);
            if(!PasswordManager.VerifyPassword(request.Password, user.Pwd))
                return Unauthorized(new { Success = false, Msg = "Bad username or password." });

            var token = _tokenService.GenerateToken(user.UserName, user.Email);
            return Ok(new { token });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _mysql.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email))
                return BadRequest(new { Success = false, Msg = "Username or email taken." });

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                Pwd = PasswordManager.HashPassword(request.Pwd),
                ProfileIMG = request.ProfileIMG
            };

            _mysql.Users.Add(user);
            await _mysql.SaveChangesAsync();

            return Ok(new {Success = true});
        }

        [HttpGet("TestMethod")]
        public IActionResult TestMethod()
        {
            return Ok(new { UserCount = _mysql.Users.Count(), MySqlConnection = "Success" });
        }

        [Authorize]
        [HttpGet("LoginCheck")]
        public IActionResult LoginCheck()
        {
            var username = User.Identity?.Name;
            return Ok($"Logged in as {username}.");
        }
    }
}
