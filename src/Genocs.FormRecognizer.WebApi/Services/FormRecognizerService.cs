using Genocs.FormRecognizer.WebApi.Options;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genocs.FormRecognizer.WebApi.Services
{
    public class FormRecognizerService
    {
        private readonly IFaceClient _client;
        private readonly ILogger<FormRecognizerService> _logger;

        public FormRecognizerService(IOptions<AzureCognitiveServicesConfig> config, ILogger<FormRecognizerService> logger)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _client = Authenticate(config.Value.Endpoint, config.Value.SubscriptionKey);
        }

        /// <summary>
        /// AUTHENTICATE
        /// Uses subscription key and region to create a client.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        private async Task<List<DetectedFace>> DetectFaceRecognize(string url)
        {
            // Detect faces from image URL. Since only recognizing, use the recognition model 1.
            // We use detection model 2 because we are not retrieving attributes.

            // Recognition model 3 was released in 2020 May.
            // It is recommended since its overall accuracy is improved
            // compared with models 1 and 2.
            IList<DetectedFace> detectedFaces = await _client.Face.DetectWithUrlAsync(url,
                                                                        recognitionModel: RecognitionModel.Recognition03,
                                                                        detectionModel: DetectionModel.Detection03);

            _logger.LogInformation($"{detectedFaces.Count} face(s) detected from image '{url}'");
            return detectedFaces.ToList();
        }

        /// <summary>
        /// This function will take an image and find a similar one to it in another image.
        /// </summary>
        /// <param name="firstImage">The first image</param>
        /// <param name="secondImage">The second image</param>
        /// <returns></returns>
        public async Task<IList<SimilarFace>> FindSimilar(string firstImage, string secondImage)
        {
            // Detect faces from source image url.
            IList<DetectedFace> sourceFaces = await DetectFaceRecognize(firstImage);

            // Detect faces from target image url.
            IList<DetectedFace> targetFaces = await DetectFaceRecognize(secondImage);

            // Add detected faceId to list of GUIDs.
            IList<Guid?> targetFaceIds = new List<Guid?>();
            foreach (var target in targetFaces)
            {
                targetFaceIds.Add(target.FaceId.Value);
            }

            // Find a similar face(s) in the list of IDs. Comaparing only the first in list for testing purposes.

            List<SimilarFace> result = new();

            foreach(var source in sourceFaces)
            {
                result.AddRange(await _client.Face.FindSimilarAsync(source.FaceId.Value, null, null, targetFaceIds));
            }

            return result;
        }

    }
}
