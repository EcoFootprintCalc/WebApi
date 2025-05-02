using EcoFootprintCalculator.Lib;
using EcoFootprintCalculator.Models.AppModels;
using EcoFootprintCalculator.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EcoFootprintCalculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            #region Auth
            builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddSingleton<TokenService>();

            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            builder.Services.AddAuthorization();
            #endregion

            #region EntityFramework
            //dotnet ef dbcontext scaffold "server=;port=3306;database=EcoFootprintCalc;user=ecofootprintuser;password=;" Pomelo.EntityFrameworkCore.MySql --output-dir Models/DbModels --context-dir Lib --context MySQL --use-database-names --no-onconfiguring --force

            builder.Services.AddDbContext<MySQL>(options =>options.UseMySql(builder.Configuration.GetConnectionString("MySQL"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQL"))));
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

//VPS only
#if DEBUG

#else
            app.UsePathBase("/EcoFootprintCalculator");
            app.UseRouting();
#endif
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
