using Azure;
using Azure.AI.FormRecognizer.Models;
using Genocs.FormRecognizer.Contracts;
using Genocs.Integration.ML.CognitiveServices.Extensions;
using Genocs.Integration.ML.CognitiveServices.Interfaces;
using Genocs.Integration.ML.CognitiveServices.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Genocs.Integration.ML.CognitiveServices.Services;

public class ImageClassifierService : IImageClassifier, IDisposable
{
    private readonly ImageClassifierConfig _config;

    private readonly ILogger<ImageClassifierService> _logger;

    // Track whether Dispose has been called.
    private bool _disposed = false;

    private HttpClient _httpClient;

    // https://westeurope.api.cognitive.microsoft.com/customvision/v3.0/Prediction/83db127e-d786-4662-8f11-4dce83da21a5/classify/iterations/Iteration1/url
    private readonly string prefix_url = "customvision/v3.0/Prediction";
    private readonly string postfix_url = "classify/iterations/Iteration1/url";

    public ImageClassifierService(ILogger<ImageClassifierService> logger, IOptions<ImageClassifierConfig> config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (config.Value == null)
        {
            throw new ArgumentNullException(nameof(config.Value));
        }

        if (!string.IsNullOrWhiteSpace(config.Value.Endpoint))
        {
            throw new ArgumentNullException(nameof(config.Value.Endpoint));
        }

        if (!string.IsNullOrWhiteSpace(config.Value.ModelId))
        {
            throw new ArgumentNullException(nameof(config.Value.ModelId));
        }

        if (!string.IsNullOrWhiteSpace(config.Value.PredictionKey))
        {
            throw new ArgumentNullException(nameof(config.Value.PredictionKey));
        }

        _config = config.Value;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(config.Value.Endpoint)
        };

        _httpClient.DefaultRequestHeaders.Add("Prediction-Key", _config.PredictionKey);
    }

    public async Task<Classification?> Classify(string url)
    {
        // The model Id is the classification model Id 
        var postResponse = await _httpClient.PostAsync($"/{prefix_url}/{_config.ModelId}/{postfix_url}", new { Url = url }.AsJson());
        postResponse.EnsureSuccessStatusCode();


        if (postResponse != null && postResponse.IsSuccessStatusCode)
        {
            return await postResponse.Content.ReadFromJsonAsync<Classification>();
        }
        return null;
    }


    /// <summary>
    /// Implement IDisposable.
    /// Do not make this method virtual.
    /// A derived class should not be able to override this method.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SuppressFinalize to
        // take this object off the finalization queue
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.

            // Note disposing has been done.
            _disposed = true;
        }
    }


    private async Task<List<dynamic>> Evaluate(RecognizeCustomFormsOperation operation)
    {
        Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
        RecognizedFormCollection forms = operationResponse.Value;

        List<dynamic> res = new();
        foreach (RecognizedForm form in forms)
        {
            _logger.LogInformation($"Form of type: {form.FormType}");
            _logger.LogInformation($"Form analyzed with model ID: {form.ModelId}");

            dynamic exo = new System.Dynamic.ExpandoObject();

            foreach (FormField field in form.Fields.Values)
            {
                ((IDictionary<string, object>)exo).Add(field.Name, new { Value = field?.ValueData?.Text, field?.Confidence });
            }
            res.Add(exo);

        }
        return res;
    }
}
