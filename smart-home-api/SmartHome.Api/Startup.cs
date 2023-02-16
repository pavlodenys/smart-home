using SmartHome.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.Logic;
using SmartHome.Data.AutoMapper;
using SmartHome.Data.Entities;
using SmartHome.Data.DTO;
using Newtonsoft;
using Microsoft.Extensions.Options;

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
            services.AddDbContext<Context>(options => options
                    .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<Context>();
            services.AddSingleton(typeof(IRepository<Sensor, SensorDto>), typeof(Repository<Sensor, SensorDto>));
            services.AddSingleton(typeof(IRepository<Device, DeviceDto>), typeof(Repository<Device, DeviceDto>));
            services.AddSingleton(typeof(IRepository<Scenario, ScenarioDto>), typeof(Repository<Scenario, ScenarioDto>));
            services.AddAutoMapper(typeof(SensorProfile));

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
                var context = serviceScope.ServiceProvider.GetRequiredService<Context>();

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
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
