using Genocs.FormRecognizer.WebApi.Dto;
using Genocs.Integration.MSAzure.Models;
using Genocs.Integration.MSAzure.Services;
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
    [Route("[controller]")]
    public class ScanFormController : ControllerBase
    {
        private readonly FormRecognizerService formRecognizerService;
        private readonly StorageService storageService;
        private readonly FormClassifierService formClassifierService;

        public ScanFormController(FormRecognizerService formRecognizerService, StorageService storageService, FormClassifierService formClassifierService)
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
        public async Task<List<dynamic>> PostUploadAndScanImage([FromQuery] string modelId, [FromForm(Name = "images")] List<IFormFile> files)
        {
            var uploadResult = await this.storageService.UploadFilesAsync(files);
            return await this.formRecognizerService.ScanRemote(modelId, uploadResult.First().URL);
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
        public async Task<List<dynamic>> PostScanImage([FromQuery] string modelId, [FromQuery] string url)
        {
            return await this.formRecognizerService.ScanRemote(modelId, url);
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
        public async Task<FormExtractorResult> GetScanImage(string url)
        {
            string iterationModelId = "83db127e-d786-4662-8f11-4dce83da21a5";
            string classificationModelId = "1ca8195a-f5e6-41c1-83f3-034df7f3a6ff";

            FormExtractorResult result = new FormExtractorResult();
            var classification = await this.formClassifierService.Classify(iterationModelId, HttpUtility.HtmlDecode(url));

            if (classification != null && classification.Predictions != null && classification.Predictions.Any())
            {
                var first = classification.Predictions.OrderByDescending(o => o.Probability).First();
            }

            result.Classification = classification;
            result.ContentData = await this.formRecognizerService.ScanRemote(classificationModelId, url);

            return result;
        }
    }
}
