using Genocs.FormRecognizer.WebApi.Extensions;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Options;
using Genocs.Integration.CognitiveServices.Services;
using Genocs.Monitoring;
using MassTransit;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ML;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console());

//builder.InitializeOpenTelemetry();
// add services to DI container
var services = builder.Services;

// Set Custom Open telemetry
services.AddCustomOpenTelemetry(builder.Configuration);

services.AddCors();
services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

services.AddHealthChecks();

services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

//Multipart
services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 60000000;
});

// Add Masstransit bus configuration
services.AddCustomMassTransit(builder.Configuration);


services.AddOptions();

services.Configure<AzureCognitiveServicesSettings>(builder.Configuration.GetSection(AzureCognitiveServicesSettings.Position));
services.Configure<AzureStorageSettings>(builder.Configuration.GetSection(AzureStorageSettings.Position));
services.Configure<ImageClassifierSettings>(builder.Configuration.GetSection(ImageClassifierSettings.Position));
services.Configure<AzureCognitiveServicesSettings>(builder.Configuration.GetSection(AzureCognitiveServicesSettings.Position));

// ML Engine Poll
string? passportModelUrl = builder.Configuration.GetSection("AppSettings")?.GetValue(typeof(string), "PassportModel")?.ToString();
if(!string.IsNullOrWhiteSpace(passportModelUrl))
{
    services.AddPredictionEnginePool<Genocs.FormRecognizer.WebApi.MachineLearnings.Passport_MLModel.ModelInput,
                                        Genocs.FormRecognizer.WebApi.MachineLearnings.Passport_MLModel.ModelOutput>()
                                .FromUri(passportModelUrl);
}

//services.Configure<RabbitMQSettings>(builder.Configuration.GetSection(RabbitMQSettings.Position));


services.AddSingleton<StorageService>();
services.AddSingleton<IFormRecognizer, FormRecognizerService>();
services.AddSingleton<IImageClassifier, ImageClassifierService>();
services.TryAddScoped<IIDocumentRecognizer, IdDocumentService>();
services.TryAddScoped<IFaceRecognizer, FaceRecognizerService>();

services.AddCustomCache(builder.Configuration);

services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// global cors policy
app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();

Log.CloseAndFlush();
