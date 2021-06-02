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

        public ScanFormController(FormRecognizerService formRecognizerService, StorageService storageService, ImageClassifierService formClassifierService)
        {
            this.formRecognizerService = formRecognizerService ?? throw new ArgumentNullException(nameof(formRecognizerService));
            this.storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            this.formClassifierService = formClassifierService ?? throw new ArgumentNullException(nameof(formClassifierService));
        }

        /// <summary>
        /// It allows to upload an image and scan it
        /// </summary>
        /// <param name="modelId">The ML ModelId</param>
        /// <param name="files">The File/s stream</param>
        /// <returns>The result</returns>
        [Route("upload_and_run"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<List<dynamic>> PostUploadAndScanImage([FromForm(Name = "images")] List<IFormFile> files)
        {
            var uploadResult = await this.storageService.UploadFilesAsync(files);
            return await this.formRecognizerService.ScanRemote(uploadResult.First().URL);
        }


        /// <summary>
        /// It allows to scan a image previously uploaded
        /// </summary>
        /// <param name="modelId">The ML ModelId</param>
        /// <param name="url">The public available url</param>
        /// <returns>The result</returns>

        [Route("run_remote"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<dynamic>> PostScanImage([FromQuery] string url)
        {
            return await this.formRecognizerService.ScanRemote(url);
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
            }

            result.Classification = classification;
            result.ContentData = await this.formRecognizerService.ScanRemote(url);

            return result;
        }
    }
}
