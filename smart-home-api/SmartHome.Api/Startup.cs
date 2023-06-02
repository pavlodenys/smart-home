using SmartHome.Data;
using SmartHome.Logic;
using SmartHome.Data.AutoMapper;
using SmartHome.Data.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using SmartHome.Api.Utilities;
using SmartHome.Api.Hubs;
using System.Diagnostics;
using SmartHome.Api.Worker;

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

            //Entity classes and Dto classes names should starts from common world. Othewise, you will add a lot of code.
            var typeEntity = typeof(DataAssembly).Assembly.GetTypes()
                .Where(x => x.FullName != null && x.FullName.Contains("Entities") && !x.IsAbstract && !x.IsInterface)
                .ToList();

            foreach (var typeDto in typeof(DataAssembly).Assembly.GetTypes().Where(x => x.Name.EndsWith("Dto") && !x.IsAbstract && !x.IsInterface))
            {
                var entityClass = typeEntity.FirstOrDefault(et => typeDto.Name.StartsWith(et.Name));
                if (entityClass == null) continue;

                var repositoryType = typeof(Repository<,>).MakeGenericType(entityClass, typeDto);
                var repositoryInterface = typeof(IRepository<,>).MakeGenericType(entityClass,typeDto);

                services.AddSingleton(repositoryInterface, repositoryType);
            }

            services.AddSingleton<ScenarioService>();
            services.AddSingleton<Services>();

            services.AddSingleton<ScenariosQueue>();

            services.AddHostedService<ScenarioConsumer>();
            services.AddHostedService<ScenarioProducer>();

            services.AddAutoMapper(typeof(DeviceProfile));

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
