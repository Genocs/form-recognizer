using Genocs.Integration.CognitiveServices.Extensions;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Models;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// 
/// </summary>
public class CardIdRecognizerService : IIDocumentRecognizer, IDisposable
{
    private readonly AzureCognitiveServicesSettings _config;
    private readonly ILogger<CardIdRecognizerService> _logger;

    // Track whether Dispose has been called.
    private bool _disposed = false;

    private HttpClient _httpClient;

    private readonly string prefix_url = "vision/v2.0/recognizeText?mode=Printed";


    public CardIdRecognizerService(IOptions<AzureCognitiveServicesSettings> config, ILogger<CardIdRecognizerService> logger)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _config = config.Value;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_config.Endpoint)
        };

        // Request headers.
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<IDResult?> RecognizeAsync(string url)
    {
        HttpResponseMessage response;

        // Two REST API methods are required to extract text.
        // One method to submit the image for processing, the other method
        // to retrieve the text found in the image.

        // operationLocation stores the URI of the second REST API method,
        // returned by the first REST API method.
        string operationLocation;

        // The first REST API method, Batch Read, starts
        // the async process to analyze the written text in the image.
        var postResponse = await _httpClient.PostAsync($"/{prefix_url}", new { Url = url }.AsJson());

        // The response header for the Batch Read method contains the URI
        // of the second method, Read Operation Result, which
        // returns the results of the process in the response body.
        // The Batch Read operation does not return anything in the response body.
        if (postResponse.IsSuccessStatusCode)
        {
            operationLocation =
                postResponse.Headers.GetValues("Operation-Location").FirstOrDefault();
        }
        else
        {
            // Display the JSON error data.
            string errorString = await postResponse.Content.ReadAsStringAsync();
            _logger.LogError($"Response: {0}", JToken.Parse(errorString).ToString());
            return null;
        }

        // If the first REST API method completes successfully, the second 
        // REST API method retrieves the text written in the image.
        //
        // Note: The response may not be immediately available. Text
        // recognition is an asynchronous operation that can take a variable
        // amount of time depending on the length of the text.
        // You may need to wait or retry this operation.
        //
        // This example checks once per second for ten seconds.
        string contentString;
        int i = 0;
        do
        {
            System.Threading.Thread.Sleep(1000);
            response = await _httpClient.GetAsync(operationLocation);
            contentString = await response.Content.ReadAsStringAsync();
            ++i;
        }
        while (i < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);

        if (i == 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1)
        {
            _logger.LogError("Timeout error.");
            return null;
        }

        var tmp = JsonConvert.DeserializeObject<CardIdResult>(contentString);

        return new IDResult(IDValidationResultType.EMPTY_DATA);
    }
}
