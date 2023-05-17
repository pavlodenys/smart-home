using Microsoft.AspNetCore;
using System.Diagnostics;

namespace SmartHome.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            Console.WriteLine("API starts.");
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
