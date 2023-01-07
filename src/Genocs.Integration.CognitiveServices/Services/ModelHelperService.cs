using Azure;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.Training;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Integration.CognitiveServices.Services;

public class ModelHelperService : IModelHelper
{
    private readonly AzureCognitiveServicesSettings _config;
    private readonly ILogger<ModelHelperService> _logger;

    private readonly FormTrainingClient _client;

    public ModelHelperService(IOptions<AzureCognitiveServicesSettings> config, ILogger<ModelHelperService> logger)
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

        if (!AzureCognitiveServicesSettings.IsValid(config.Value))
        {
            throw new ArgumentException("AzureCognitiveServicesSettings is invalid", nameof(config.Value));
        }

        _config = config.Value;
        _client = new FormTrainingClient(new Uri(_config.Endpoint), new AzureKeyCredential(_config.SubscriptionKey));
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


    //private async Task GetInfoFromCustomFormUrl(string modelId, string url)
    //{

    //    Uri formFileUri = new Uri(url);

    //    RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsFromUriAsync(modelId, formFileUri);
    //    Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
    //    RecognizedFormCollection forms = operationResponse.Value;

    //    foreach (RecognizedForm form in forms)
    //    {
    //        _logger.LogInformation($"Form of type: {form.FormType}");
    //        _logger.LogInformation($"Form was analyzed with model with ID: {form.ModelId}");
    //        foreach (FormField field in form.Fields.Values)
    //        {
    //            _logger.LogInformation($"Field '{field.Name}': ");

    //            if (field.LabelData != null)
    //            {
    //                _logger.LogInformation($"  Label: '{field.LabelData.Text}'");
    //            }

    //            _logger.LogInformation($"  Value: '{field.ValueData.Text}'");
    //            _logger.LogInformation($"  Confidence: '{field.Confidence}'");
    //        }
    //    }
    //}

    //private async Task TestLocal(string endpoint, string apiKey, string modelId, string filePath)
    //{

    //    using var stream = new FileStream(filePath, FileMode.Open);

    //    RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsAsync(modelId, stream);
    //    Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
    //    RecognizedFormCollection forms = operationResponse.Value;

    //    foreach (RecognizedForm form in forms)
    //    {
    //        _logger.LogInformation($"Form of type: {form.FormType}");
    //        _logger.LogInformation($"Form was analyzed with model with ID: {form.ModelId}");
    //        foreach (FormField field in form.Fields.Values)
    //        {
    //            _logger.LogInformation($"Field '{field.Name}': ");

    //            if (field.LabelData != null)
    //            {
    //                _logger.LogInformation($"  Label: '{field.LabelData.Text}'");
    //            }

    //            _logger.LogInformation($"  Value: '{field.ValueData.Text}'");
    //            _logger.LogInformation($"  Confidence: '{field.Confidence}'");
    //        }
    //    }
    //}

    public async Task CreateModelAsync(string trainingSetUrl, string modelName)
    {
        // Create a new model to store in the account
        Uri trainingSetUri = new Uri(trainingSetUrl);

        TrainingOperation operation = _client.StartTraining(trainingSetUri, useTrainingLabels: true, modelName);
        Response<CustomFormModel> operationResponse = await operation.WaitForCompletionAsync();
        CustomFormModel model = operationResponse.Value;

        // Get the model that was just created
        CustomFormModel modelCopy = _client.GetCustomModel(model.ModelId);

        _logger.LogInformation($"Custom Model with Id {modelCopy.ModelId} and name {modelCopy.ModelName} recognizes the following form types:");

        foreach (CustomFormSubmodel submodel in modelCopy.Submodels)
        {
            _logger.LogInformation($"Submodel Form Type: {submodel.FormType}");
            foreach (CustomFormModelField field in submodel.Fields.Values)
            {
                _logger.LogInformation($"  FieldName: {field.Name}");
                if (field.Label != null)
                {
                    _logger.LogInformation($", FieldLabel: {field.Label}/n");
                }
            }
        }

        //// Delete the model from the account.
        //client.DeleteModel(model.ModelId);
    }

    public void EvaluateExisting()
    {
        // Check number of models in the FormRecognizer account, and the maximum number of models that can be stored.
        AccountProperties accountProperties = _client.GetAccountProperties();
        _logger.LogInformation($"Account has {accountProperties.CustomModelCount} models.");
        _logger.LogInformation($"It can have at most {accountProperties.CustomModelLimit} models.");

        // List the first ten or fewer models currently stored in the account.
        Pageable<CustomFormModelInfo> models = _client.GetCustomModels();

        foreach (CustomFormModelInfo modelInfo in models.Take(10))
        {
            _logger.LogInformation($"Custom Model Info:");
            _logger.LogInformation($"  Model Id: {modelInfo.ModelId}");
            _logger.LogInformation($"  Model name: {modelInfo.ModelName}");
            _logger.LogInformation($"  Is composed model: {modelInfo.Properties.IsComposedModel}");
            _logger.LogInformation($"  Model Status: {modelInfo.Status}");
            _logger.LogInformation($"  Training model started on: {modelInfo.TrainingStartedOn}");
            _logger.LogInformation($"  Training model completed on: {modelInfo.TrainingCompletedOn}");
        }
    }
}
