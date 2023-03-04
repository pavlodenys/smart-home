using SmartHome.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.Logic;
using SmartHome.Data.AutoMapper;
using SmartHome.Data.Entities;
using SmartHome.Data.DTO;
using Newtonsoft;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SmartHome.Api
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            services.AddDbContext<SmartHomeDbContext>(options => options
                    .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<SmartHomeDbContext>();
            services.AddSingleton(typeof(IRepository<Sensor, SensorDto>), typeof(Repository<Sensor, SensorDto>));
            services.AddSingleton(typeof(IRepository<Device, DeviceDto>), typeof(Repository<Device, DeviceDto>));
            services.AddSingleton(typeof(IRepository<Scenario, ScenarioDto>), typeof(Repository<Scenario, ScenarioDto>));
            services.AddAutoMapper(typeof(SensorProfile));

            var tokenValidatorParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = tokenValidatorParameters;
                    //options.Events = new JwtBearerEvents
                    //{
                    //    OnChallenge = context =>
                    //    {
                    //       // context.Response.Headers["WWW-Authenticate"] = "Bearer";
                    //        if (context.Request.Headers["Authorization"].Count == 0)
                    //        {
                    //            context.Response.StatusCode = 401;
                    //        }
                    //        else
                    //        {
                    //            context.Response.StatusCode = 403;
                    //        }
                    //        return Task.CompletedTask;
                    //    }
                    //};
                });

            services.AddSingleton<IService, Services>();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // var context = app.ApplicationServices.GetRequiredService<Context>();
            app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<SmartHomeDbContext>();

                if (context.Database.EnsureCreated())
                {
                    context.Database.Migrate();
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
