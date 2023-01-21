﻿using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// FormRecognizerService implementation
/// </summary>
public class FormRecognizerService : IFormRecognizer
{
    private readonly AzureCognitiveServicesSettings _config;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<FormRecognizerService> _logger;

    private readonly FormRecognizerClient _client;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="distributedCache"></param>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FormRecognizerService(IDistributedCache distributedCache, IOptions<AzureCognitiveServicesSettings> config, ILogger<FormRecognizerService> logger)
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
        string? classificationModelId = await _distributedCache.GetStringAsync(classificationKey);

        if (string.IsNullOrWhiteSpace(classificationModelId))
        {
            throw new NullReferenceException($"DistributedCache do not contains classificationKey: '{classificationKey}'");
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new NullReferenceException($"filePath cannot be null or empty");
        }

        using var stream = new FileStream(filePath, FileMode.Open);
        RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsAsync(classificationModelId, stream);
        return await Evaluate(operation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classificationKey">_The Classification key</param>
    /// <param name="url">the resource url</param>
    /// <returns>dynamic list result</returns>
    /// <exception cref="NullReferenceException"></exception>
    public async Task<List<dynamic>> ScanRemote(string classificationKey, string url)
    {
        string? classificationModelId = await _distributedCache.GetStringAsync(classificationKey);

        if (string.IsNullOrWhiteSpace(classificationModelId))
        {
            throw new NullReferenceException($"DistributedCache do not contains classificationKey: '{classificationKey}'");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new NullReferenceException($"url cannot be null or empty");
        }

        Uri formFileUri = new Uri(url);
        RecognizeCustomFormsOperation operation = await _client.StartRecognizeCustomFormsFromUriAsync(classificationModelId, formFileUri);
        return await Evaluate(operation);
    }

    public async Task<string?> ScanLocalCardId(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new NullReferenceException($"filePath cannot be null or empty");
        }

        using var stream = new FileStream(filePath, FileMode.Open);
        var options = new RecognizeIdentityDocumentsOptions() { ContentType = FormContentType.Jpeg };

        RecognizeIdentityDocumentsOperation operation = await _client.StartRecognizeIdentityDocumentsAsync(stream, options);
        Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
        RecognizedFormCollection identityDocuments = operationResponse.Value;
        RecognizedForm identityDocument = identityDocuments.Single();

        return identityDocument.FormType;
    }

    /// <summary>
    /// Scan image to find ID Document
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public async Task<string?> ScanRemoteCardIdAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new NullReferenceException($"url cannot be null or empty");

        }
        Uri formFileUri = new Uri(url);
        RecognizeIdentityDocumentsOperation operation = await _client.StartRecognizeIdentityDocumentsFromUriAsync(formFileUri);
        Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();

        RecognizedFormCollection identityDocuments = operationResponse.Value;
        RecognizedForm identityDocument = identityDocuments.Single();

        string? mrz = null;
        if (identityDocument.FormType == "prebuilt:idDocument:passport")
        {
            if (identityDocument.FormTypeConfidence > 0.90)
            {
                if (identityDocument.Fields["MachineReadableZone"] != null && identityDocument.Fields["MachineReadableZone"].ValueData != null)
                {
                    mrz = identityDocument.Fields["MachineReadableZone"].ValueData.Text;
                }
            }
        }
        return mrz;
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
