using Genocs.FormRecognizer.ConsoleApp.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Genocs.FormRecognizer.ConsoleApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Requires `using Microsoft.Extensions.Hosting;`
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost => configHost.AddEnvironmentVariables())
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddEnvironmentVariables();

                    var env = context.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();
                    }

                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // Requires `using Microsoft.Extensions.Logging;`
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddApplicationInsights();

                    var serilogBuilder = new LoggerConfiguration()
                                                    .ReadFrom
                                                    .Configuration(hostingContext.Configuration)
                                                    .WriteTo
                                                    .Console(new CompactJsonFormatter());

                    logging.AddSerilog(serilogBuilder.CreateLogger(), true);

                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.Configure<AzureCognitiveServicesConfig>(hostingContext.Configuration.GetSection("AzureCognitiveServicesConfig"));
                    services.AddHostedService<WorkerService>();
                });

        }

    }
}
