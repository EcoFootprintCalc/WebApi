using EcoFootprintCalculator.Lib;
using EcoFootprintCalculator.Models.DbModels;
using EcoFootprintCalculator.Models.HttpModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoFootprintCalculator.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : BaseController
    {
        public UserController(MySQL _mysql) : base(_mysql) { }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if(u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            if (!PasswordManager.VerifyPassword(request.OldPassword, u.Pwd))
                return BadRequest(new { Success = false, Msg = "Old password is incorrect!"});

            u.Pwd = PasswordManager.HashPassword(request.NewPassword);
            _mysql.Users.Update(u);
            await _mysql.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            User? u = await _mysql.Users.SingleOrDefaultAsync(u => u.ID == logonId);
            if (u is null)
                return BadRequest(new { Success = false, Msg = "User not found!" });

            return Ok(new { Success = true, User = new { ProfileIMG = u.ProfileIMG, Email = u.Email, UserName = u.UserName, ID = u.ID } });
        }

        [HttpPost("AddCar")]
        public async Task<IActionResult> AddCar([FromBody] AddCarRequest request)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var car = new Car()
            {
                UserID = logonId,
                Name = request.Name,
                AvgFuelConsumption = request.AvgFuelConsumption,
                Brand = request.Brand,
                Type = request.Type
            };

            _mysql.Cars.Add(car);
            await _mysql.SaveChangesAsync();

            return Ok(new { Success = true, Msg = $"Car successfully added with ID: {car.ID}", RecordID = car.ID});
        }

        [HttpGet("GetCars")]
        public IActionResult GetCars()
        {
            return Ok(new {Success = true, Cars = _mysql.Cars.Where(c=>c.UserID == logonId)});
        }
    }
}
