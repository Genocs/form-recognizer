using Genocs.FormRecognizer.Worker;
using Genocs.Integration.CognitiveServices.Options;
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

        // Register Settings
        services.Configure<AzureCognitiveServicesSettings>(hostContext.Configuration.GetSection(AzureCognitiveServicesSettings.Position));
        services.Configure<AzureStorageSettings>(hostContext.Configuration.GetSection(AzureStorageSettings.Position));
        services.Configure<ImageClassifierSettings>(hostContext.Configuration.GetSection(ImageClassifierSettings.Position));
        services.Configure<AzureCognitiveServicesSettings>(hostContext.Configuration.GetSection(AzureCognitiveServicesSettings.Position));

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
