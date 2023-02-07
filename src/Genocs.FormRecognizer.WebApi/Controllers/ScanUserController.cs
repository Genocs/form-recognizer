using Genocs.FormRecognizer.WebApi.Models;
using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Models;
using Genocs.Integration.CognitiveServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using System.Net.Mime;

namespace Genocs.FormRecognizer.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class ScanUserController : ControllerBase
{
    private readonly IFaceRecognizer _faceRecognizerService;
    private readonly IIDocumentRecognizer _idDocumentService;
    private readonly StorageService _storageService;
    private readonly PredictionEnginePool<MachineLearnings.Passport_MLModel.ModelInput,
                                        MachineLearnings.Passport_MLModel.ModelOutput> _predictionEnginePool;

    public ScanUserController(StorageService storageService,
                                IFaceRecognizer faceRecognizerService,
                                IIDocumentRecognizer idDocumentService,
                                PredictionEnginePool<MachineLearnings.Passport_MLModel.ModelInput, MachineLearnings.Passport_MLModel.ModelOutput> predictionEnginePool)
    {
        _faceRecognizerService = faceRecognizerService ?? throw new ArgumentNullException(nameof(faceRecognizerService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _idDocumentService = idDocumentService ?? throw new ArgumentNullException(nameof(idDocumentService));
        _predictionEnginePool = predictionEnginePool ?? throw new ArgumentNullException(nameof(predictionEnginePool));
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

        // Copy for local predictionEngine
        Stream s = images[0].OpenReadStream();

        MachineLearnings.Passport_MLModel.ModelInput inputData = new MachineLearnings.Passport_MLModel.ModelInput();

        using (BinaryReader br = new BinaryReader(s))
        {
            inputData.ImageSource = br.ReadBytes((int)s.Length);
        }

        MachineLearnings.Passport_MLModel.ModelOutput outputData = _predictionEnginePool.Predict(inputData);


        // Check if the first image contains the ID
        var idDocumentResult = await _idDocumentService.RecognizeAsync(uploadResult.First().URL);

        if (idDocumentResult is null)
        {
            // ID not found on first image, check on the second one
            idDocumentResult = await _idDocumentService.RecognizeAsync(uploadResult.Last().URL);
        }

        // No ID found. Stop here any other checks
        if (idDocumentResult is null)
        {
            return BadRequest("no document found in the images");
        }

        // Run the face match
        var faceResult = await _faceRecognizerService.CompareFacesAsync(uploadResult.First().URL, uploadResult.Last().URL);

        if (faceResult is null || !faceResult.Any())
        {
            return BadRequest("no face into the images");
        }

        MemberScanResponse response = new MemberScanResponse(outputData.PredictedLabel,
                                                              outputData.Score[0],
                                                              faceResult.OrderBy(o => o.Confidence).FirstOrDefault().Confidence,
                                                              idDocumentResult);

        return Ok(response);
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
    public async Task<ActionResult<List<dynamic>>> PostEvaluateAsync([FromBody] MemberScanRequest request)
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


        await _faceRecognizerService.CompareFacesAsync(request.IdDocumentImageUrl, request.FaceImageUrl);



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
        return result?.ValidationResult != IDValidationResultType.VALID ? NoContent() : Ok(result);
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

public class MemberScanRequest
{
    public string IdDocumentImageUrl { get; set; }
    public string FaceImageUrl { get; set; }
}

public class MemberScanResponse
{
    public FaceResult Face { get; set; }
    public IDResult IdDocument { get; set; }
    public string DocumentData { get; set; }
    public float DocumentDataScore { get; set; }

    public string Overall { get; set; }

    public MemberScanResponse(string documentData, float documentDataScore, double faceScore, IDResult idDocumentResult)
    {
        DocumentData = documentData;
        DocumentDataScore = documentDataScore;
        Face = new FaceResult(faceScore);
        IdDocument = idDocumentResult;
        Overall = DocumentData;

        if (DocumentData != "valid")
        {
            return;
        }

        if (IdDocument.ValidationResult != IDValidationResultType.VALID)
        {
            Overall = IdDocument.ValidationResult.ToString();
            return;
        }

        if (!Face.Match)
        {
            Overall = "face mismatch";
            return;
        }

        if (Face.Score > 0.99)
        {
            Overall = "face duplicated";
            return;
        }
    }

    public record FaceResult
    {
        public bool Match { get; init; }

        public double Score { get; init; }

        public FaceResult(double score)
        {
            Match = score > 0.55f;
            Score = score;
        }
    }

    public record IdDocumentResult
    {
        public IDValidationResultType FormatType { get; init; }

    }
}
