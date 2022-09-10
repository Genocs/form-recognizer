using Genocs.FormRecognizer.Service;
using Genocs.Integration.ML.CognitiveServices.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        OpenTelemetryInitializer.Initialize(hostContext, services);

        // Register config
        services.Configure<AzureCognitiveServicesConfig>(hostContext.Configuration.GetSection("AzureCognitiveServicesConfig"));
        services.Configure<AzureStorageConfig>(hostContext.Configuration.GetSection("AzureStorageConfig"));
        services.Configure<ImageClassifierConfig>(hostContext.Configuration.GetSection("ImageClassifierConfig"));
        services.Configure<AzureCognitiveServicesConfig>(hostContext.Configuration.GetSection("FormRecognizerConfig"));

        services.AddHostedService<WorkerService>();

    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddSerilog(dispose: true);
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();

Log.CloseAndFlush();
