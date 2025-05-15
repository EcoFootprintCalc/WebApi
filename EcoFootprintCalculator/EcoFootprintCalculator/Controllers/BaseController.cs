using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EcoFootprintCalculator.Lib;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcoFootprintCalculator.Controllers
{
    public abstract class BaseController : Controller
    {
        protected MySQL _mysql { get; private set; }

        protected int logonId = -1;

        public BaseController(MySQL sql)
        {
            this._mysql = sql;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
            {
                var token = authorizationHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (emailClaim != null)
                {
                    if (_mysql.Users.Any(u => u.Email == emailClaim.Value))
                        logonId = _mysql.Users.Single(u => u.Email == emailClaim.Value).ID;
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
