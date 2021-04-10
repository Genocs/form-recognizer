using Genocs.FormRecognizer.WebApi.Dto;
using Genocs.Integration.MSAzure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genocs.FormRecognizer.WebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ScanFormController : ControllerBase
    {
        private readonly FormRecognizerService formRecognizerService;
        private readonly StorageService storageService;

        public ScanFormController(FormRecognizerService formRecognizerService, StorageService storageService)
        {
            this.formRecognizerService = formRecognizerService ?? throw new ArgumentNullException(nameof(formRecognizerService));
            this.storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        /// <summary>
        /// It allows to upload an image and scan it
        /// </summary>
        /// <param name="files">Images files</param>
        /// <returns>Result</returns>
        [Route("uploadtoexecute"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<FormRecognizerResult> PostUploadAndScanImage([FromForm(Name = "images")] List<IFormFile> files)
        {
            var uploadResult = await this.storageService.UploadFilesAsync(files);
            IList<Microsoft.Azure.CognitiveServices.Vision.Face.Models.SimilarFace> similarResult = await this.formRecognizerService.FindSimilar(uploadResult.First().URL, uploadResult.Last().URL);

            return await Task.Run(() =>
            {
                FormRecognizerResult result = new();
                if (similarResult != null && similarResult.Any())
                {
                    result.Confidence = similarResult.OrderByDescending(c => c.Confidence).First().Confidence;
                }
                return result;
            });
        }


        /// <summary>
        /// It allows to scan a image previously uploaded
        /// </summary>
        /// <param name="files">Images files</param>
        /// <returns>Result</returns>
        [Route("execute"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<FormRecognizerResult> PostScanImage(string url)
        {

            //IList<Microsoft.Azure.CognitiveServices.Vision.Face.Models.SimilarFace> similarResult = await this.formRecognizerService.FindSimilar(uploadResult.First().URL, uploadResult.Last().URL);

            return await Task.Run(() =>
            {
                FormRecognizerResult result = new();
                //if (similarResult != null && similarResult.Any())
                //{
                //    result.Confidence = similarResult.OrderByDescending(c => c.Confidence).First().Confidence;
                //}
                return result;
            });
        }
    }
}
