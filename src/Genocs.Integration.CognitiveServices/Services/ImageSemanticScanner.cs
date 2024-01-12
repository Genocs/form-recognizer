using Azure;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Integration.CognitiveServices.Services;

public class ImageSemanticScanner : IImageSemanticScanner
{
    private readonly AzureVisionSettings _config;
    private readonly ILogger<ImageSemanticScanner> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="config">The config object instance.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">This exception is thrown in case mandatory data are missing.</exception>
    /// <exception cref="ArgumentException">This exception is thrown in case setting info are incorrect or missing.</exception>
    public ImageSemanticScanner(IOptions<AzureVisionSettings> config, ILogger<ImageSemanticScanner> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (!AzureVisionSettings.IsValid(config.Value, true))
        {
            throw new ArgumentException("AzureVisionSettings is invalid", nameof(config.Value));
        }

        _config = config.Value;
    }

    /// <summary>
    /// The standard implementation.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<List<dynamic>> ScanAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new NullReferenceException(nameof(url));
        }

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new InvalidDataException($"resource url '{url}' is invalid");
        }

        _logger.LogInformation($"Called RecognizeAsync: '{url}'");

        AzureKeyCredential credential = new AzureKeyCredential(_config.SubscriptionKey);

        //var serviceOptions = new VisionServiceOptions(new Uri(_config.Endpoint), credential);

        //using var imageSource = VisionSource.FromUrl(new Uri(url));

        //var analysisOptions = new ImageAnalysisOptions()
        //{
        //    Features = ImageAnalysisFeature.DenseCaptions | ImageAnalysisFeature.Objects,
        //    Language = "en",
        //    GenderNeutralCaption = true
        //};

        //using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

        //var result = await analyzer.AnalyzeAsync();

        //if (result.Reason == ImageAnalysisResultReason.Analyzed)
        //{
        //    //if (result.Caption != null)
        //    //{
        //    //    Console.WriteLine(" Caption:");
        //    //    Console.WriteLine($"   \"{result.Caption.Content}\", Confidence {result.Caption.Confidence:0.0000}");
        //    //}

        //    //if (result.Text != null)
        //    //{
        //    //    Console.WriteLine($" Text:");
        //    //    foreach (var line in result.Text.Lines)
        //    //    {
        //    //        string pointsToString = "{" + string.Join(',', line.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
        //    //        Console.WriteLine($"   Line: '{line.Content}', Bounding polygon {pointsToString}");

        //    //        foreach (var word in line.Words)
        //    //        {
        //    //            pointsToString = "{" + string.Join(',', word.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
        //    //            Console.WriteLine($"     Word: '{word.Content}', Bounding polygon {pointsToString}, Confidence {word.Confidence:0.0000}");
        //    //        }
        //    //    }
        //    //}
        //}
        //else
        //{
        //    var errorDetails = ImageAnalysisErrorDetails.FromResult(result);
        //}

        return new List<dynamic> { "result" };
    }
}
