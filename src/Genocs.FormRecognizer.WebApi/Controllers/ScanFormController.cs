using Genocs.FormRecognizer.WebApi.Models;
using Genocs.Integration.CognitiveServices.Contracts;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Web;

namespace Genocs.FormRecognizer.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class ScanFormController : ControllerBase
{
    private readonly IFormRecognizer _formRecognizerService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IImageClassifier _formClassifierService;
    private readonly StorageService _storageService;

    public ScanFormController(StorageService storageService,
                                IFormRecognizer formRecognizerService,
                                IImageClassifier formClassifierService,
                                IPublishEndpoint publishEndpoint)
    {
        _formRecognizerService = formRecognizerService ?? throw new ArgumentNullException(nameof(formRecognizerService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _formClassifierService = formClassifierService ?? throw new ArgumentNullException(nameof(formClassifierService));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    /// <summary>
    /// It allows to classify an image.
    /// </summary>
    /// <param name="url">The HTML encoded url</param>
    /// <returns>The classification result</returns>
    [Route("Classify"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Prediction))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClassifyAsync([FromBody] BasicRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest("url cannot be null or empty");
        }

        var classification = await _formClassifierService.ClassifyAsync(HttpUtility.HtmlDecode(request.Url));

        if (classification != null && classification.Predictions != null && classification.Predictions.Any())
        {
            var first = classification.Predictions.OrderByDescending(o => o.Probability).First();

            return Ok(first);
        }
        return NoContent();
    }


    /// <summary>
    /// It allows to upload an image and classify it
    /// </summary>
    /// <param name="files">File that will be classified</param>
    /// <returns>The Prediction result</returns>
    [Route("UploadAndClassify"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Prediction))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostUploadAndClassifyAsync([FromForm(Name = "images")] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("no 'images' file detected");
        }

        if (files[0].Length == 0)
        {
            return BadRequest("empty 'images' file not allowed");
        }

        // Upload on storage
        var uploadResult = await _storageService.UploadFilesAsync(files);

        // Classify the result
        var classification = await _formClassifierService.ClassifyAsync(HttpUtility.HtmlDecode(uploadResult.First().URL));

        if (classification != null && classification.Predictions != null && classification.Predictions.Any())
        {
            var first = classification.Predictions.OrderByDescending(o => o.Probability).First();

            return Ok(first);
        }
        return NoContent();
    }


    /// <summary>
    /// It allows to upload an image and estract form data it
    /// </summary>
    /// <param name="files">The File/s stream</param>
    /// <param name="classificationModelId">The classification model Id</param>
    /// <returns>The result</returns>
    [Route("UploadAndEvaluate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<dynamic>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<dynamic>>> PostUploadAndEvaluateAsync([FromForm(Name = "images")] List<IFormFile> files, [FromQuery] string classificationModelId)
    {
        if (string.IsNullOrWhiteSpace(classificationModelId))
        {
            return BadRequest("classificationModelId cannot be null or empty");
        }

        var uploadResult = await _storageService.UploadFilesAsync(files);
        return await _formRecognizerService.ScanAsync(classificationModelId, uploadResult.First().URL);
    }


    /// <summary>
    /// It allows to scan a image previously uploaded
    /// </summary>
    /// <param name="modelId">The ML ModelId</param>
    /// <param name="url">The public available url</param>
    /// <returns>The result</returns>

    [Route("Evaluate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ActionResult<List<dynamic>>> PostClassifyAndEvaluateAsync([FromBody] EvaluateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClassificationModelId))
        {
            return BadRequest("classificationModelId cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest("url cannot be null or empty");
        }

        return await _formRecognizerService.ScanAsync(request.ClassificationModelId, request.Url);
    }


    /// <summary>
    /// It allows to scan a image previously uploaded
    /// </summary>
    /// <returns>The result</returns>
    [Route("ClassifyAndEvaluate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FormExtractorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetClassifyAndEvaluateAsync([FromBody] BasicRequest request)
    {
        if (request == null)
        {
            return BadRequest("request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest("request Url cannot be null or empty");
        }


        FormExtractorResponse result = new FormExtractorResponse();
        result.ResourceUrl = HttpUtility.HtmlDecode(request.Url);

        if (string.IsNullOrWhiteSpace(result.ResourceUrl))
        {
            return BadRequest("request Url is null or empty after HtmlDecode");
        }

        var classification = await _formClassifierService.ClassifyAsync(result.ResourceUrl);

        if (classification != null && classification.Predictions != null && classification.Predictions.Any())
        {
            var firstPrediction = classification.Predictions.OrderByDescending(o => o.Probability).First();
            result.ContentData = await _formRecognizerService.ScanAsync(firstPrediction.TagId!, request.Url);
        }

        result.Classification = classification;

        // Publish to the service bus 
        await _publishEndpoint.Publish(result);

        return Ok(result);
    }
}

