namespace Genocs.Integration.CognitiveServices.Options;

/// <summary>
/// The ImageClassifierSettings object
/// </summary>
public class ImageClassifierSettings
{
    /// <summary>
    /// Default Section name
    /// </summary>
    public const string Position = "ImageClassifierSettings";
    public string? Endpoint { get; set; }
    public string? PredictionId { get; set; }
    public string? PredictionKey { get; set; }
    public string? ModelId { get; set; }
}
