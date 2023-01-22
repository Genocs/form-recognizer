using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Models;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// 
/// </summary>
public class IdDocumentService : IIDocumentRecognizer, IDisposable
{
    private readonly AzureCognitiveServicesSettings _config;
    private readonly ILogger<IdDocumentService> _logger;

    // Track whether Dispose has been called.
    private bool _disposed = false;


    /// <summary>
    /// Standard constructor
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public IdDocumentService(IOptions<AzureCognitiveServicesSettings> config, ILogger<IdDocumentService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _config = config.Value;

        if (string.IsNullOrWhiteSpace(_config.SubscriptionKey))
        {
            throw new ArgumentNullException(_config.SubscriptionKey);
        }

        if (string.IsNullOrWhiteSpace(_config.Endpoint))
        {
            throw new ArgumentNullException(_config.Endpoint);
        }

        if (!Uri.IsWellFormedUriString(_config.Endpoint, UriKind.Absolute))
        {
            throw new InvalidDataException($"Config Endpoint Uri '{_config.Endpoint}' is invalid");
        }
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
    /// The standard implementation
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<IDResult?> RecognizeAsync(string url)
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
        DocumentAnalysisClient documentAnalysisClient = new DocumentAnalysisClient(new Uri(_config.Endpoint), credential);
        AnalyzeDocumentOperation operationResult = await documentAnalysisClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, IdDocumentHelper.MODEL_ID, new Uri(url));

        Azure.AI.FormRecognizer.DocumentAnalysis.AnalyzeResult? analysisResult = null;
        if (!operationResult.HasCompleted)
        {
            var response = await operationResult.WaitForCompletionAsync();
            analysisResult = response.Value;
        }

        analysisResult = operationResult.Value;

        if (analysisResult == null)
        {
            return null;
        }

        if (analysisResult.Documents == null || !analysisResult.Documents.Any())
        {
            // No documents
            return null;
        }

        AnalyzedDocument? document = analysisResult.Documents.OrderBy(o => o.Confidence).FirstOrDefault();

        // Extract fields
        IDValidationResultType validationResult = IdDocumentHelper.Validate(document);

        IDResult result = new IDResult
        {
            ValidationResult = validationResult,
            Raw = analysisResult
        };

        if (validationResult == IDValidationResultType.VALID)
        {
            result.Number = document.Fields["DocumentNumber"].Value.AsString();
        }

        return result;
    }
}
