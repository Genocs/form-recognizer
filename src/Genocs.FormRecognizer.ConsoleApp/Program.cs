using Genocs.Integration.ML.CognitiveServices.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

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
            return Host.CreateDefaultBuilder(args)
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
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                    var serilogBuilder = new LoggerConfiguration()
                                                    .ReadFrom
                                                    .Configuration(hostingContext.Configuration)
                                                    .WriteTo
                                                    .Console(new CompactJsonFormatter());

                    logging.AddSerilog(serilogBuilder.CreateLogger(), true);

                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    // Register config
                    services.Configure<AzureCognitiveServicesConfig>(hostingContext.Configuration.GetSection("AzureCognitiveServicesConfig"));
                    services.Configure<AzureStorageConfig>(hostingContext.Configuration.GetSection("AzureStorageConfig"));
                    services.Configure<ImageClassifierConfig>(hostingContext.Configuration.GetSection("ImageClassifierConfig"));
                    services.Configure<AzureCognitiveServicesConfig>(hostingContext.Configuration.GetSection("FormRecognizerConfig"));

                    // Register services
                    services.AddHostedService<WorkerService>();
                });
        }
    }
}
