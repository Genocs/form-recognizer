using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Models;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Integration.CognitiveServices.Services;

public class IdDocumentService : ICardIdRecognizer, IDisposable
{
    private readonly AzureCognitiveServicesSettings _config;
    private readonly ILogger<IdDocumentService> _logger;

    // Track whether Dispose has been called.
    private bool _disposed = false;


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

    public async Task<CardIdResult> Recognize(string url)
    {

        AzureKeyCredential credential = new AzureKeyCredential(_config.SubscriptionKey);
        DocumentAnalysisClient documentAnalysisClient = new DocumentAnalysisClient(new Uri(_config.Endpoint), credential);
        var res = await documentAnalysisClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-idDocument", new Uri(url));
        //CardIdResult result = new CardIdResult { AnalyzeResult = }

        return null;
    }

}
