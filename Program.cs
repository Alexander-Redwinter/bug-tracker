using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BugTracker
{
    public class Program
    {
        /// <summary>
        /// Main entry point for application
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
