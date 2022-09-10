namespace Genocs.Integration.ML.CognitiveServices.Options;

public class ImageClassifierConfig
{
    public const string Position = "ImageClassifierConfig";
    public string? Endpoint { get; set; }
    public string? PredictionId { get; set; }
    public string? PredictionKey { get; set; }
    public string? ModelId { get; set; }
}
