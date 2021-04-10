using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Genocs.Integration.MSAzure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Genocs.Integration.MSAzure.Services
{
    public class FormRecognizerService
    {
        private readonly ILogger<FormRecognizerService> _logger;
        private readonly FormRecognizerClient _client;

        public FormRecognizerService(IOptions<AzureCognitiveServicesConfig> config, ILogger<FormRecognizerService> logger)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AzureCognitiveServicesConfig cfg = config.Value;
            _client = new FormRecognizerClient(new Uri(cfg.Endpoint), new AzureKeyCredential(cfg.SubscriptionKey));
        }

        public async Task ScanLocal(string modelId, string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open);
            RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsAsync(modelId, stream);

            await Evaluate(operation);
        }

        public async Task ScanRemote(string modelId, string url)
        {
            Uri formFileUri = new Uri(url);
            RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsFromUriAsync(modelId, formFileUri);
            await Evaluate(operation); 
        }

        private async Task Evaluate(RecognizeCustomFormsOperation operation)
        {
            Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
            RecognizedFormCollection forms = operationResponse.Value;

            foreach (RecognizedForm form in forms)
            {
                _logger.LogInformation($"Form of type: {form.FormType}");
                _logger.LogInformation($"Form was analyzed with model with ID: {form.ModelId}");
                foreach (FormField field in form.Fields.Values)
                {
                    _logger.LogInformation($"Field '{field.Name}': ");

                    if (field.LabelData != null)
                    {
                        _logger.LogInformation($"  Label: '{field.LabelData.Text}'");
                    }

                    _logger.LogInformation($"  Value: '{field.ValueData.Text}'");
                    _logger.LogInformation($"  Confidence: '{field.Confidence}'");
                }
            }
        }
    }
}
