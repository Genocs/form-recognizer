using Genocs.FormRecognizer.Worker;
using Genocs.FormRecognizer.Worker.Options;
using Genocs.Integration.CognitiveServices.Options;
using Genocs.Monitoring;
using MassTransit;
using Serilog;
using Serilog.Events;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, builder) =>
    {
        builder.AddUserSecrets<Program>();
    })
    .ConfigureServices((hostContext, services) =>
    {
        // TelemetryAndLogging.Initialize(hostContext.Configuration.GetConnectionString("ApplicationInsights"));
        services.AddCustomOpenTelemetry(hostContext.Configuration);

        ConfigureMongoDb(services, hostContext.Configuration);
        ConfigureMassTransit(services, hostContext.Configuration);

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

static IServiceCollection ConfigureMongoDb(IServiceCollection services, IConfiguration configuration)
{
    //services.Configure<DBSettings>(configuration.GetSection(DBSettings.Position));

    //services.TryAddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
    //services.TryAddSingleton<IRepository<Order, string>, MongoDbRepositoryBase<Order, string>>();

    // Add Repository here

    return services;
}

static IServiceCollection ConfigureMassTransit(IServiceCollection services, IConfiguration configuration)
{
    // services.AddMediator();

    var rabbitMQSettings = new RabbitMQSettings();
    configuration.GetSection(RabbitMQSettings.Position).Bind(rabbitMQSettings);

    services.AddSingleton(rabbitMQSettings);

    // services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

    services.AddMassTransit(x =>
    {
        // Consumer configuration
        //x.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

        // Consumer
        x.AddConsumers(Assembly.GetExecutingAssembly());
        x.AddActivities(Assembly.GetExecutingAssembly());
        x.SetKebabCaseEndpointNameFormatter();

        // Transport RabbitMQ
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
            //cfg.UseHealthCheck(context);
            cfg.Host(rabbitMQSettings.HostName, rabbitMQSettings.VirtualHost,
                h =>
                {
                    h.Username(rabbitMQSettings.UserName);
                    h.Password(rabbitMQSettings.Password);

                    //h.UseSsl(s =>
                    //{
                    //    s.Protocol = SslProtocols.Tls12;
                    //});
                }
            );
        });

        //// Set the transport
        //x.UsingRabbitMq(ConfigureBus);
    });

    return services;
}

static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
{
    //configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));

    //configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
    //{
    //    e.PrefetchCount = 20;

    //    e.Batch<RoutingSlipCompleted>(b =>
    //    {
    //        b.MessageLimit = 10;
    //        b.TimeLimit = TimeSpan.FromSeconds(5);

    //        b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
    //    });
    //});

    // This configuration allow to handle the Scheduling
    configurator.UseMessageScheduler(new Uri("queue:quartz"));

    // This configuration will configure the Activity Definition
    configurator.ConfigureEndpoints(context);

    //cfg.UseHealthCheck(context);
    //configurator.Host(rabbitMQSettings.HostName, rabbitMQSettings.VirtualHost,
    //    h =>
    //    {
    //        h.Username(rabbitMQSettings.UserName);
    //        h.Password(rabbitMQSettings.Password);

    //        h.UseSsl(s =>
    //        {
    //            s.Protocol = SslProtocols.Tls12;
    //        });
    //    }
    //);

}