using FluentValidation;
using Genocs.Core.Builders;
using Genocs.FormRecognizer.WebApi.Controllers;
using Genocs.FormRecognizer.WebApi.Extensions;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Options;
using Genocs.Integration.CognitiveServices.Services;
using Genocs.Logging;
using Genocs.Tracing;
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

builder.Host
        .UseLogging();

// add services to DI container
var services = builder.Services;

services
    .AddGenocs(builder.Configuration)
    .AddOpenTelemetry();

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

services.AddOptions();

// Add Masstransit bus configuration
services.AddCustomMassTransit(builder.Configuration);

// Multipart
services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 60000000;
});

services.Configure<AzureCognitiveServicesSettings>(builder.Configuration.GetSection(AzureCognitiveServicesSettings.Position));
services.Configure<AzureVisionSettings>(builder.Configuration.GetSection("AzureVision"));
services.Configure<AzureStorageSettings>(builder.Configuration.GetSection(AzureStorageSettings.Position));
services.Configure<ImageClassifierSettings>(builder.Configuration.GetSection(ImageClassifierSettings.Position));

// ML Engine Poll
string? passportModelUrl = builder.Configuration.GetSection("AppSettings")?.GetValue(typeof(string), "PassportModel")?.ToString();
if (!string.IsNullOrWhiteSpace(passportModelUrl))
{
    services.AddPredictionEnginePool<Genocs.FormRecognizer.WebApi.MachineLearnings.Passport_MLModel.ModelInput,
                                        Genocs.FormRecognizer.WebApi.MachineLearnings.Passport_MLModel.ModelOutput>()
                                .FromUri(passportModelUrl);
}

// services.Configure<RabbitMQSettings>(builder.Configuration.GetSection(RabbitMQSettings.Position));

services.AddSingleton<StorageService>();
services.AddSingleton<IFormRecognizer, FormRecognizerService>();
services.AddSingleton<IImageClassifier, ImageClassifierService>();
services.TryAddScoped<IIDocumentRecognizer, IdDocumentService>();
services.TryAddScoped<IImageSemanticScanner, ImageSemanticScanner>();

// services.TryAddScoped<IFaceRecognizer, FaceRecognizerService>();

services.AddCustomCache(builder.Configuration);

services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

// Fluent Validation
services.AddValidatorsFromAssemblyContaining<SetupSettingRequestValidator>();

var app = builder.Build();

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

app.MapHealthChecks("/hc");

app.Run();

Log.CloseAndFlush();
