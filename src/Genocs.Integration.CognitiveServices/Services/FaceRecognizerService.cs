using Genocs.Integration.CognitiveServices.Interfaces;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// FaceReconizerService implementation
/// </summary>
public class FaceRecognizerService : IFaceRecognizer
{
    private readonly AzureCognitiveServicesSettings _config;
    private readonly IFaceClient _client;
    private readonly ILogger<FaceRecognizerService> _logger;

    /// <summary>
    /// Standard Constructor
    /// </summary>
    /// <param name="config">The settings</param>
    /// <param name="logger">The logger</param>
    /// <exception cref="ArgumentNullException"></exception>
    public FaceRecognizerService(IOptions<AzureCognitiveServicesSettings> config, ILogger<FaceRecognizerService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _config = config.Value;

        if (string.IsNullOrWhiteSpace(_config.SubscriptionKey))
        {
            throw new ArgumentNullException(_config.SubscriptionKey);
        }

        if (string.IsNullOrWhiteSpace(_config.Endpoint))
        {
            throw new ArgumentNullException(_config.Endpoint);
        }

        if (!Uri.IsWellFormedUriString(_config.Endpoint, UriKind.Absolute))
        {
            throw new InvalidDataException($"Config Endpoint Uri '{_config.Endpoint}' is invalid");
        }

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

    private async Task<List<DetectedFace>> RecognizeFaceAsync(string url)
    {
        // Detect faces from image URL. Since only recognizing, use the recognition model 1.
        // We use detection model 2 because we are not retrieving attributes.

        // Recognition model 3 was released in 2020 May.
        // It is recommended since its overall accuracy is improved
        // compared with models 1 and 2.

        // Recognition model 4 was released in 2021 February.
        // It is recommended since its accuracy is improved
        // on faces wearing masks compared with model 3,
        // and its overall accuracy is improved compared
        // with models 1 and 2.

        IList<DetectedFace> detectedFaces = await _client.Face.DetectWithUrlAsync(url: url,
                                                                                    returnFaceId: null,
                                                                                    returnFaceLandmarks: false,
                                                                                    returnFaceAttributes: null,
                                                                                    recognitionModel: RecognitionModel.Recognition04,
                                                                                    returnRecognitionModel: false,
                                                                                    detectionModel: DetectionModel.Detection03);

        _logger.LogInformation($"{detectedFaces.Count} face(s) detected from image '{url}'");
        return detectedFaces.ToList();
    }

    private async Task<List<DetectedFace>> RecognizeFaceAsync(Stream stream)
    {
        // Detect faces from image URL. Since only recognizing, use the recognition model 1.
        // We use detection model 2 because we are not retrieving attributes.

        // Recognition model 3 was released in 2020 May.
        // It is recommended since its overall accuracy is improved
        // compared with models 1 and 2.

        // Recognition model 4 was released in 2021 February.
        // It is recommended since its accuracy is improved
        // on faces wearing masks compared with model 3,
        // and its overall accuracy is improved compared
        // with models 1 and 2.

        IList<DetectedFace> detectedFaces = await _client.Face.DetectWithStreamAsync(stream,
                                                                    recognitionModel: RecognitionModel.Recognition04,
                                                                    detectionModel: DetectionModel.Detection03);

        _logger.LogInformation($"{detectedFaces.Count} face(s) detected from stream");
        return detectedFaces.ToList();
    }

    /// <summary>
    /// This function will take an image and find a similar one to it in another image.
    /// </summary>
    /// <param name="firstImage">The first image</param>
    /// <param name="secondImage">The second image</param>
    /// <returns></returns>
    public async Task<IList<SimilarFace>> CompareFacesAsync(string firstImage, string secondImage)
    {
        // Detect faces from source image url.
        IList<DetectedFace> sourceFaces = await RecognizeFaceAsync(firstImage);

        // Detect faces from target image url.
        IList<DetectedFace> targetFaces = await RecognizeFaceAsync(secondImage);

        // Add detected faceId to list of GUIDs.
        IList<Guid?> targetFaceIds = new List<Guid?>();

        List<SimilarFace> result = new();

        if (!targetFaces.Any())
        {
            return result;
        }

        foreach (var target in targetFaces)
        {
            if (target.FaceId != null)
            {
                targetFaceIds.Add(target.FaceId.Value);
            }
        }

        // Find a similar face(s) in the list of IDs. Comparing only the first in list for testing purposes.
        foreach (var source in sourceFaces)
        {
            if (source.FaceId != null)
            {
                result.AddRange(await _client.Face.FindSimilarAsync(faceId: source.FaceId.Value,
                                                                        faceListId: null,
                                                                        largeFaceListId: null,
                                                                        faceIds: targetFaceIds,
                                                                        maxNumOfCandidatesReturned: 2,
                                                                        mode: FindSimilarMatchMode.MatchPerson,
                                                                        cancellationToken: default));
            }
        }
        return result;
    }
}
