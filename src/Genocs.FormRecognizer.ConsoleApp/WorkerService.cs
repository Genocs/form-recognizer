using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.Training;
using Genocs.Integration.MSAzure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genocs.FormRecognizer.ConsoleApp
{
    class WorkerService : IHostedService, IDisposable
    {
        private bool _disposed;

        private readonly ILogger<WorkerService> _logger;
        private readonly AzureCognitiveServicesConfig _configCognitiveServices;
        private readonly AzureStorageConfig _configAzureStorage;

        public WorkerService(ILogger<WorkerService> logger,
            IOptions<AzureCognitiveServicesConfig> configCognitiveServices,
            IOptions<AzureStorageConfig> configAzureStorage)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configCognitiveServices = configCognitiveServices.Value;
            _configAzureStorage = configAzureStorage.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await MainAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping gracefully");
            return Task.CompletedTask;
        }


        /// <summary>
        /// Dispose() calls Dispose(true)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// The bulk of the clean-up code is implemented in Dispose(bool)
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // free managed resources
            }

            // free native resources if there are any.

            _disposed = true;
        }



        private async Task MainAsync()
        {
            string endpoint = _configCognitiveServices.Endpoint;
            string apiKey = _configCognitiveServices.SubscriptionKey;
            string trainingSetUrl = _configAzureStorage.TrainingSetContainer;
            string fileUrl = _configAzureStorage.InspectingFileUrl;

            string modelId = "40763499-a146-4202-be20-0418510ae1e4";
            string modelName = "2021_04_08_01";
            string filePath = @"C:\tmp\uno.jpg";

            EvaluateExisting(endpoint, apiKey);

            // Remove the comment on the line below to create a new model
            //await CreateModel(endpoint, apiKey, trainingSetUrl, modelName);
            await TestLocal(endpoint, apiKey, modelId, filePath);
            await TestUrl(endpoint, apiKey, modelId, fileUrl);

            _logger.LogInformation("Done Successfully!");
        }


        private static void EvaluateExisting(string endpoint, string apiKey)
        {
            FormTrainingClient client = new FormTrainingClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            // Check number of models in the FormRecognizer account, and the maximum number of models that can be stored.
            AccountProperties accountProperties = client.GetAccountProperties();
            Console.WriteLine($"Account has {accountProperties.CustomModelCount} models.");
            Console.WriteLine($"It can have at most {accountProperties.CustomModelLimit} models.");

            // List the first ten or fewer models currently stored in the account.
            Pageable<CustomFormModelInfo> models = client.GetCustomModels();

            foreach (CustomFormModelInfo modelInfo in models.Take(10))
            {
                Console.WriteLine($"Custom Model Info:");
                Console.WriteLine($"  Model Id: {modelInfo.ModelId}");
                Console.WriteLine($"  Model name: {modelInfo.ModelName}");
                Console.WriteLine($"  Is composed model: {modelInfo.Properties.IsComposedModel}");
                Console.WriteLine($"  Model Status: {modelInfo.Status}");
                Console.WriteLine($"  Training model started on: {modelInfo.TrainingStartedOn}");
                Console.WriteLine($"  Training model completed on: {modelInfo.TrainingCompletedOn}");
            }
        }

        private static async Task CreateModel(string endpoint, string apiKey, string trainingSetUrl, string modelName)
        {
            // Create a new model to store in the account
            Uri trainingSetUri = new Uri(trainingSetUrl);

            FormTrainingClient client = new FormTrainingClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            TrainingOperation operation = client.StartTraining(trainingSetUri, useTrainingLabels: true, modelName);
            Response<CustomFormModel> operationResponse = await operation.WaitForCompletionAsync();
            CustomFormModel model = operationResponse.Value;

            // Get the model that was just created
            CustomFormModel modelCopy = client.GetCustomModel(model.ModelId);

            Console.WriteLine($"Custom Model with Id {modelCopy.ModelId}  and name {modelCopy.ModelName} recognizes the following form types:");

            foreach (CustomFormSubmodel submodel in modelCopy.Submodels)
            {
                Console.WriteLine($"Submodel Form Type: {submodel.FormType}");
                foreach (CustomFormModelField field in submodel.Fields.Values)
                {
                    Console.Write($"  FieldName: {field.Name}");
                    if (field.Label != null)
                    {
                        Console.Write($", FieldLabel: {field.Label}");
                    }
                    Console.WriteLine("");
                }
            }

            //// Delete the model from the account.
            //client.DeleteModel(model.ModelId);
        }


        private static async Task TestLocal(string endpoint, string apiKey, string modelId, string filePath)
        {

            FormRecognizerClient client = new FormRecognizerClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            using var stream = new FileStream(filePath, FileMode.Open);

            RecognizeCustomFormsOperation operation = await client.StartRecognizeCustomFormsAsync(modelId, stream);
            Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
            RecognizedFormCollection forms = operationResponse.Value;

            foreach (RecognizedForm form in forms)
            {
                Console.WriteLine($"Form of type: {form.FormType}");
                Console.WriteLine($"Form was analyzed with model with ID: {form.ModelId}");
                foreach (FormField field in form.Fields.Values)
                {
                    Console.WriteLine($"Field '{field.Name}': ");

                    if (field.LabelData != null)
                    {
                        Console.WriteLine($"  Label: '{field.LabelData.Text}'");
                    }

                    Console.WriteLine($"  Value: '{field.ValueData.Text}'");
                    Console.WriteLine($"  Confidence: '{field.Confidence}'");
                }
            }
        }

        private static async Task TestUrl(string endpoint, string apiKey, string modelId, string url)
        {
            FormRecognizerClient client = new FormRecognizerClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            Uri formFileUri = new Uri(url);

            RecognizeCustomFormsOperation operation = await client.StartRecognizeCustomFormsFromUriAsync(modelId, formFileUri);
            Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
            RecognizedFormCollection forms = operationResponse.Value;

            foreach (RecognizedForm form in forms)
            {
                Console.WriteLine($"Form of type: {form.FormType}");
                Console.WriteLine($"Form was analyzed with model with ID: {form.ModelId}");
                foreach (FormField field in form.Fields.Values)
                {
                    Console.WriteLine($"Field '{field.Name}': ");

                    if (field.LabelData != null)
                    {
                        Console.WriteLine($"  Label: '{field.LabelData.Text}'");
                    }

                    Console.WriteLine($"  Value: '{field.ValueData.Text}'");
                    Console.WriteLine($"  Confidence: '{field.Confidence}'");
                }
            }
        }
    }
}
