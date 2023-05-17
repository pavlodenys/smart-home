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
using Microsoft.AspNetCore.Identity;
using SmartHome.Api.Utilities;
using SmartHome.Api.Worker;
using Microsoft.AspNetCore.SignalR;
using SmartHome.Api.Hubs;
using System.Diagnostics;

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
            //services.AddDbContext<SmartHomeDbContext>(options => options
            //        .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<SmartHomeDbContext>();
            services.AddSingleton(typeof(IRepository<Sensor, SensorDto>), typeof(Repository<Sensor, SensorDto>));
            services.AddSingleton(typeof(IRepository<Device, DeviceDto>), typeof(Repository<Device, DeviceDto>));
            services.AddSingleton(typeof(IRepository<Scenario, ScenarioDto>), typeof(Repository<Scenario, ScenarioDto>));
            services.AddSingleton(typeof(IRepository<Point, PointDto>), typeof(Repository<Point, PointDto>));
            services.AddSingleton(typeof(IRepository<HomeUser, HomeUserDto>), typeof(Repository<HomeUser, HomeUserDto>));
            services.AddSingleton(typeof(IRepository<RefreshToken, RefreshTokenDto>), typeof(Repository<RefreshToken, RefreshTokenDto>));

            services.AddSingleton<ScenarioService>();
            services.AddSingleton<Services>();

            services.AddSingleton<ScenariosQueue>();

            //services.AddHostedService<ScenarioConsumer>();
            //wservices.AddHostedService<ScenarioProducer>();

            services.AddAutoMapper(typeof(SensorProfile));

            services.AddIdentityCore<HomeUser>((setup) =>
            {
                setup.Password.RequireDigit = true;
                setup.Password.RequiredLength = 8;
                setup.Password.RequireUppercase = true;
            })
                //.AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SmartHomeDbContext>()
                .AddDefaultTokenProviders();

            services.AddHttpContextAccessor();
            services.AddScoped<SignInManager<HomeUser>>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            var tokenValidatorParameters = new TokenValidationParameters
            {
                //ValidateIssuer = true,
                //ValidateAudience = true,
                //ValidateLifetime = true,
                //ValidateIssuerSigningKey = true,
                //ValidIssuer = Configuration["Jwt:Issuer"],
                //ValidAudience = Configuration["Jwt:Audience"],
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key"] ?? "jwt_key")),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            services.AddSignalR();

            services.AddRouting();

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin"));
            });//todo: check

            services.AddSingleton<IService, Services>();

            services.AddControllers().AddJsonOptions(options =>
            {
                //options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            Debug.WriteLine("Configuration.....Startup1");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // var context = app.ApplicationServices.GetRequiredService<Context>();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            //using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            //{
            //    var context = serviceScope.ServiceProvider.GetRequiredService<SmartHomeDbContext>();

            //    if (context.Database.EnsureCreated())
            //    {
            //        context.Database.Migrate();
            //    }
            //}

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Debug.WriteLine("Configuration.....Startup2");

            // app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "backend v1"));

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<SensorsHub>("/hub");
                endpoints.MapControllers();
            });
        }
    }
}
