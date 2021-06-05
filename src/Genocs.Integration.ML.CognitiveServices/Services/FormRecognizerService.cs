using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Genocs.Integration.ML.CognitiveServices.Interfaces;
using Genocs.Integration.ML.CognitiveServices.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Genocs.Integration.ML.CognitiveServices.Services
{
    public class FormRecognizerService : IFormRecognizer
    {
        private readonly FormRecognizerConfig _config;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<FormRecognizerService> _logger;

        private readonly FormRecognizerClient _client;

        public FormRecognizerService(IDistributedCache distributedCache, IOptions<FormRecognizerConfig> config, ILogger<FormRecognizerService> logger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _config = config.Value;

            _client = new FormRecognizerClient(new Uri(_config.Endpoint), new AzureKeyCredential(_config.SubscriptionKey));
        }

        public async Task<List<dynamic>> ScanLocal(string classificationKey, string filePath)
        {
            string classificationModelId = await _distributedCache.GetStringAsync(classificationKey);

            using var stream = new FileStream(filePath, FileMode.Open);
            RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsAsync(classificationModelId, stream);
            return await Evaluate(operation);
        }

        public async Task<List<dynamic>> ScanRemote(string classificationKey, string url)
        {
            string classificationModelId = await _distributedCache.GetStringAsync(classificationKey);

            Uri formFileUri = new Uri(url);
            RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsFromUriAsync(classificationModelId, formFileUri);
            return await Evaluate(operation);
        }

        private async Task<List<dynamic>> Evaluate(RecognizeCustomFormsOperation operation)
        {
            Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
            RecognizedFormCollection forms = operationResponse.Value;

            List<dynamic> res = new();
            foreach (RecognizedForm form in forms)
            {
                _logger.LogInformation($"Form of type: {form.FormType}");
                _logger.LogInformation($"Form was analyzed with model with ID: {form.ModelId}");

                dynamic exo = new System.Dynamic.ExpandoObject();

                foreach (FormField field in form.Fields.Values)
                {
                    ((IDictionary<string, object>)exo).Add(field.Name, new { Value = field?.ValueData?.Text, field?.Confidence });
                }
                res.Add(exo);


                //foreach (FormField field in form.Fields.Values)
                //{
                //    _logger.LogInformation($"Field '{field.Name}': ");

                //    if (field.LabelData != null)
                //    {
                //        _logger.LogInformation($"  Label: '{field.LabelData.Text}'");
                //    }

                //    _logger.LogInformation($"  Value: '{field.ValueData.Text}'");
                //    _logger.LogInformation($"  Confidence: '{field.Confidence}'");
                //}
            }
            return res;
        }
    }
}
