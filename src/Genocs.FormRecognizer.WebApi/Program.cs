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
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;
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

// Register the Swagger generator, defining 1 or more Swagger documents
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Form Recognizer Web API",
        Description = "The Form Recognizer is a part of Fiscanner platform. The API contains OpenAPI documentation to be used easily by LLM Orchestrator like LangChain, Semantic Kernel or AutoGen.",
        TermsOfService = new Uri("https://www.genocs.com/sections/software.html"),
        Contact = new OpenApiContact
        {
            Name = "Giovanni Emanuele Nocco",
            Email = "giovanni.nocco@gmail.com",
            Url = new Uri("https://www.genocs.com"),
        },
        License = new OpenApiLicense
        {
            Name = "Use under MIT",
            Url = new Uri("https://opensource.org/license/mit/"),
        }
    });

    c.AddServer(new OpenApiServer() { Url = "http://localhost:5200", Description = "Local to be used over http" });
    c.AddServer(new OpenApiServer() { Url = "http://localhost:5201", Description = "Local to be used over https" });

    c.AddServer(new OpenApiServer() { Url = "http://formrecognizer-webapi", Description = "To be used inside docker with docker compose" });
    c.AddServer(new OpenApiServer() { Url = "https://fiscanner-formrecognizer.azurewebsites.net", Description = "Azure deploy" });

    c.CustomOperationIds(oid =>
    {
        if (!(oid.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
        {
            return null; // default behavior
        }

        return oid.GroupName switch
        {
            "v1" => $"{actionDescriptor.ActionName}",
            _ => $"_{actionDescriptor.ActionName}", // default behavior
        };
    });

    // var filePath = Path.Combine(System.AppContext.BaseDirectory, "api-documentation.xml");
    c.IncludeXmlComments("api-documentation.xml");

});

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

/* For PRODUCTION is better to disable Swagger
 * * 
 * *
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

*/

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FormRecognizer WebAPIs");
});

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
