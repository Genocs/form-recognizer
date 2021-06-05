using Genocs.Integration.ML.CognitiveServices.Models;
using Genocs.Integration.ML.CognitiveServices.Services;
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
        private readonly FormRecognizerService formRecognizerService;
        private readonly StorageService storageService;
        private readonly ImageClassifierService formClassifierService;

        public ScanFormController(FormRecognizerService formRecognizerService,
                                    StorageService storageService,
                                    ImageClassifierService formClassifierService)
        {
            this.formRecognizerService = formRecognizerService ?? throw new ArgumentNullException(nameof(formRecognizerService));
            this.storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            this.formClassifierService = formClassifierService ?? throw new ArgumentNullException(nameof(formClassifierService));
        }

        /// <summary>
        /// It allows to classify an image.
        /// </summary>
        /// <param name="url">The HTML encoded url</param>
        /// <returns>The classification result</returns>
        [Route("Classify"), HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Prediction>> GetClassify([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("url cannot be null or empty");
            }

            var classification = await this.formClassifierService.Classify(HttpUtility.HtmlDecode(url));

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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Prediction>> PostUploadAndClassify([FromForm(Name = "images")] List<IFormFile> files)
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
            var uploadResult = await this.storageService.UploadFilesAsync(files);

            // Classify the result
            var classification = await this.formClassifierService.Classify(HttpUtility.HtmlDecode(uploadResult.First().URL));

            if (classification != null && classification.Predictions != null && classification.Predictions.Any())
            {
                var first = classification.Predictions.OrderByDescending(o => o.Probability).First();

                return first;
            }
            return null;
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
            var uploadResult = await this.storageService.UploadFilesAsync(files);
            return await this.formRecognizerService.ScanRemote(classificationModelId, uploadResult.First().URL);
        }


        /// <summary>
        /// It allows to scan a image previously uploaded
        /// </summary>
        /// <param name="modelId">The ML ModelId</param>
        /// <param name="url">The public available url</param>
        /// <returns>The result</returns>

        [Route("ClassifyAndEvalaute"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<dynamic>>> PostClassifyAndEvalaute([FromQuery] string classificationModelId, string url)
        {
            if (string.IsNullOrWhiteSpace(classificationModelId))
            {
                return BadRequest("classificationModelId cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("url cannot be null or empty");
            }

            return await this.formRecognizerService.ScanRemote(classificationModelId, url);
        }


        /// <summary>
        /// It allows to scan a image previously uploaded
        /// </summary>
        /// <param name="modelId">The ML ModelId</param>
        /// <param name="url">The public available url</param>
        /// <returns>The result</returns>
        [Route("evaluate_form"), HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<FormExtractorResult> GetScanImage(string url)
        {
            FormExtractorResult result = new();
            var classification = await this.formClassifierService.Classify(HttpUtility.HtmlDecode(url));

            if (classification != null && classification.Predictions != null && classification.Predictions.Any())
            {
                var first = classification.Predictions.OrderByDescending(o => o.Probability).First();
                result.ContentData = await this.formRecognizerService.ScanRemote(first.TagId, url);
            }

            result.Classification = classification;

            return result;
        }
    }
}
