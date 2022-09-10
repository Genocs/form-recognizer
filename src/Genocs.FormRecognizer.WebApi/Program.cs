using Genocs.FormRecognizer.WebApi;
using Genocs.FormRecognizer.WebApi.Extensions;
using Genocs.FormRecognizer.WebApi.Options;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Options;
using Genocs.Integration.CognitiveServices.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console());

// ***********************************************
// Open Telemetry - START
OpenTelemetryInitializer.Initialize(builder);
// Open Telemetry - END
// ***********************************************


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");
});

builder.Services.AddOptions();

builder.Services.Configure<AzureCognitiveServicesConfig>(builder.Configuration.GetSection(AzureCognitiveServicesConfig.Position));
builder.Services.Configure<AzureStorageConfig>(builder.Configuration.GetSection(AzureStorageConfig.Position));
builder.Services.Configure<ImageClassifierConfig>(builder.Configuration.GetSection(ImageClassifierConfig.Position));
builder.Services.Configure<AzureCognitiveServicesConfig>(builder.Configuration.GetSection(AzureCognitiveServicesConfig.Position));
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection(RabbitMQConfig.Position));


builder.Services.AddSingleton<StorageService>();
builder.Services.AddSingleton<IFormRecognizer, FormRecognizerService>();
builder.Services.AddSingleton<IImageClassifier, ImageClassifierService>();
builder.Services.AddSingleton<ICardIdRecognizer, CardIdRecognizerService>();

builder.Services.AddCustomCache(builder.Configuration.GetSection(RedisConfig.Position));


builder.Services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
builder.Services.AddMassTransit(x =>
{  

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMQOptions = new RabbitMQConfig();
        builder.Configuration.GetSection(RabbitMQConfig.Position).Bind(rabbitMQOptions);

        cfg.Host(rabbitMQOptions.URL, rabbitMQOptions.VirtualHost, h =>
        {
            h.Username(rabbitMQOptions.Username);
            h.Password(rabbitMQOptions.Password);
        });


        MessageDataDefaults.ExtraTimeToLive = TimeSpan.FromDays(1);
        MessageDataDefaults.Threshold = 2000;
        MessageDataDefaults.AlwaysWriteToRepository = false;
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

Log.CloseAndFlush();
