using Genocs.FormRecognizer.WebApi.Dto;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Models;
using Genocs.Integration.CognitiveServices.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Genocs.FormRecognizer.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class ScanUserController : ControllerBase
{
    private readonly IFaceRecognizer _faceRecognizerService;
    private readonly IIDocumentRecognizer _idDocumentService;
    private readonly StorageService _storageService;

    public ScanUserController(StorageService storageService,
                                IFaceRecognizer faceRecognizerService,
                                IIDocumentRecognizer idDocumentService)
    {
        _faceRecognizerService = faceRecognizerService ?? throw new ArgumentNullException(nameof(faceRecognizerService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _idDocumentService = idDocumentService ?? throw new ArgumentNullException(nameof(idDocumentService));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="documentImage">The ID document image file</param>
    /// <param name="faceImage">The face image file</param>
    /// <returns></returns>
    [Route("UploadAndEvaluate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostUploadAndEvaluateAsync([FromForm(Name = "images")] List<IFormFile> images)
    {
        if (images is null || images.Count != 2)
        {
            return BadRequest("images must contains 2 elements: idDocument and face image");
        }

        foreach (var image in images)
        {
            if (image is null || image.Length <= 0)
            {
                return BadRequest("images must contains 2 valid elements");
            }
        }

        // Upload on storage
        var uploadResult = await _storageService.UploadFilesAsync(images);

        // Check if the first image contains a document
        var result = await _idDocumentService.RecognizeAsync(uploadResult.First().URL);

        if (result is null)
        {
            // ID not found on first image, check on the second one
            result = await _idDocumentService.RecognizeAsync(uploadResult.Last().URL);
        }

        // No ID found. Stop here any other checks
        if (result is null)
        {
            return BadRequest("no document found in the images");
        }

        return Ok("done");
    }


    /// <summary>
    /// It allows to scan a image previously uploaded
    /// </summary>
    /// <param name="modelId">The ML ModelId</param>
    /// <param name="url">The public available url</param>
    /// <returns>The result</returns>

    [Route("Evaluate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ActionResult<List<dynamic>>> PostEvalauteAsync([FromBody] EvalauteMemberKYCRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdDocumentImageUrl))
        {
            return BadRequest("IdDocumentImageUrl cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(request.FaceImageUrl))
        {
            return BadRequest("FaceImageUrl cannot be null or empty");
        }

        // Check if the first image contains a document
        var result = await _idDocumentService.RecognizeAsync(request.IdDocumentImageUrl);

        if (result is null)
        {
            return BadRequest("IdDocumentImageUrl do not contain valid ID Document");
        }


        await _faceRecognizerService.CompareAsync(request.IdDocumentImageUrl, request.FaceImageUrl);

        return Ok("done");
    }

    /// <summary>
    /// It allows to scan a image previously uploaded
    /// </summary>
    /// <param name="url">The public available url</param>
    /// <returns>The result</returns>
    [Route("CardId"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetCardIdInfoAsync([FromBody] BasicRequest request)
    {
        var result = await _idDocumentService.RecognizeAsync(request.Url);
        return string.IsNullOrWhiteSpace(result?.AnalyzeResult?.Version) ? NoContent() : Ok(result);
    }

    /// <summary>
    /// It allows to scan previously uploaded images
    /// </summary>
    /// <param name="url">The public available url</param>
    /// <returns>The result</returns>
    [Route("IdDocument"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CardIdResult))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetIdDocumentInfoAsync([FromBody] BasicRequest request)
    {
        var result = await _idDocumentService.RecognizeAsync(request.Url);
        return result == null ? NoContent() : Ok(result);
    }
}

public class EvalauteMemberKYCRequest
{
    public string IdDocumentImageUrl { get; set; }
    public string FaceImageUrl { get; set; }
}