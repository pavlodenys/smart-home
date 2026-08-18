using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SmartHome.Data;
using System.Diagnostics;

namespace SmartHome.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            Console.WriteLine("API starts.");
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SmartHomeDbContext>();
                context.Database.Migrate();
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
