using Genocs.FormRecognizer.WebApi.Dto;
using Genocs.Integration.ML.CognitiveServices.Interfaces;
using Genocs.Integration.ML.CognitiveServices.Models;
using Genocs.Integration.ML.CognitiveServices.Services;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Genocs.FormRecognizer.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ScanFormController : ControllerBase
    {
        private readonly IFormRecognizer _formRecognizerService;
        private readonly ICardIdRecognizer _cardRecognizerService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IImageClassifier _formClassifierService;
        private readonly StorageService _storageService;

        public ScanFormController(StorageService storageService,
                                    IFormRecognizer formRecognizerService,
                                    IImageClassifier formClassifierService,
                                    ICardIdRecognizer cardRecognizerService,
                                    IPublishEndpoint publishEndpoint)
        {
            _formRecognizerService = formRecognizerService ?? throw new ArgumentNullException(nameof(formRecognizerService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _formClassifierService = formClassifierService ?? throw new ArgumentNullException(nameof(formClassifierService));
            _cardRecognizerService = cardRecognizerService ?? throw new ArgumentNullException(nameof(cardRecognizerService));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        /// <summary>
        /// It allows to classify an image.
        /// </summary>
        /// <param name="url">The HTML encoded url</param>
        /// <returns>The classification result</returns>
        [Route("Classify"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Prediction>> GetClassify([FromBody] BasicRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest("url cannot be null or empty");
            }

            var classification = await _formClassifierService.Classify(HttpUtility.HtmlDecode(request.Url));

            if (classification != null && classification.Predictions != null && classification.Predictions.Any())
            {
                var first = classification.Predictions.OrderByDescending(o => o.Probability).First();

                return first;
            }
            return null;
        }


        /// <summary>
        /// It allows to upload an image and classify it
        /// </summary>
        /// <param name="files">File that will be classified</param>
        /// <returns>The Prediction result</returns>
        [Route("UploadAndClassify"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostUploadAndClassify([FromForm(Name = "images")] List<IFormFile> files)
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
            var classification = await _formClassifierService.Classify(HttpUtility.HtmlDecode(uploadResult.First().URL));

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<dynamic>> PostUploadAndEvaluate([FromForm(Name = "images")] List<IFormFile> files, [FromQuery] string classificationModelId)
        {
            var uploadResult = await this._storageService.UploadFilesAsync(files);
            return await _formRecognizerService.ScanRemote(classificationModelId, uploadResult.First().URL);
        }


        /// <summary>
        /// It allows to scan a image previously uploaded
        /// </summary>
        /// <param name="modelId">The ML ModelId</param>
        /// <param name="url">The public available url</param>
        /// <returns>The result</returns>

        [Route("Evaluate"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<dynamic>>> PostClassifyAndEvalaute([FromBody] EvaluateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClassificationModelId))
            {
                return BadRequest("classificationModelId cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest("url cannot be null or empty");
            }

            return await this._formRecognizerService.ScanRemote(request.ClassificationModelId, request.Url);
        }


        /// <summary>
        /// It allows to scan a image previously uploaded
        /// </summary>
        /// <param name="modelId">The ML ModelId</param>
        /// <param name="url">The public available resource url</param>
        /// <returns>The result</returns>
        [Route("ClassifyAndEvaluate"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<FormExtractorResult> GetClassifyAndEvaluate([FromBody] BasicRequest request)
        {
            FormExtractorResult result = new();
            result.ResourceUrl = HttpUtility.HtmlDecode(request.Url);
            var classification = await this._formClassifierService.Classify(HttpUtility.HtmlDecode(request.Url));

            if (classification != null && classification.Predictions != null && classification.Predictions.Any())
            {
                var first = classification.Predictions.OrderByDescending(o => o.Probability).First();
                result.ContentData = await _formRecognizerService.ScanRemote(first.TagId, request.Url);
            }

            result.Classification = classification;

            // Publish to the service bus 
            await this._publishEndpoint.Publish(result);

            return result;
        }


        /// <summary>
        /// It allows to scan a image previously uploaded
        /// </summary>
        /// <param name="modelId">The ML ModelId</param>
        /// <param name="url">The public available url</param>
        /// <returns>The result</returns>
        [Route("CardId"), HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult> GetCardIdInfo([FromBody] BasicRequest request)
        {
            await _formRecognizerService.ScanRemoteCardId(request.Url);
            return NoContent();
        }
    }
}
