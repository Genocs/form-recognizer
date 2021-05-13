using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Genocs.Integration.MSAzure.Extensions;
using Genocs.Integration.MSAzure.Models;
using Genocs.Integration.MSAzure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace Genocs.Integration.MSAzure.Services
{
    public class FormClassifierService
    {
        private readonly ILogger<FormClassifierService> _logger;
        private readonly FormRecognizerClient _client;

        private readonly HttpClient _httpClient;

        // https://westeurope.api.cognitive.microsoft.com/customvision/v3.0/Prediction/83db127e-d786-4662-8f11-4dce83da21a5/classify/iterations/Iteration1/url
        private readonly string _url = "classify/iterations/Iteration1/url";

        public FormClassifierService(IOptions<AzureCognitiveServicesImageClassifierConfig> config, ILogger<FormClassifierService> logger)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AzureCognitiveServicesImageClassifierConfig cfg = config.Value;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(cfg.Endpoint)
            };

            _httpClient.DefaultRequestHeaders.Add("Prediction-Key", cfg.PredictionKey);
        }

        public async Task<Classification> Classify(string modelId, string url)
        {
            var postResponse = await _httpClient.PostAsync($"/customvision/v3.0/Prediction/{modelId}/{_url}", new { Url = url }.AsJson());
            postResponse.EnsureSuccessStatusCode();

            if (postResponse != null && postResponse.IsSuccessStatusCode)
            {
                return await postResponse.Content.ReadFromJsonAsync<Classification>();
            }
            return null;
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
}
