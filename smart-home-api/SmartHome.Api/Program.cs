using Microsoft.AspNetCore;

namespace SmartHome.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            Console.WriteLine("API starts.");

            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
