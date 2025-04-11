using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneSTools.EventLog.Exporter.Core;
using OneSTools.EventLog.Exporter.Core.UserServices;

namespace OneSTools.EventLog.Exporter.Manager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSystemd()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var logPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, "log.txt");
                    logging.AddFile(logPath);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .ConfigureServices((_, services) =>
                {
                    services.Configure<UserOptions>(_.Configuration.GetSection("UserCache"));
                    services.AddTransient<IUserService, UserService>();
                    services.AddSingleton<IUserCache, UserCache>();
                    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
                    services.AddHostedService<ExportersManager>();
                });
        }
    }
}