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
//    public string? PredictionId { get; set; }
    public string? PredictionKey { get; set; }
    public string? ModelId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static bool IsValid(ImageClassifierSettings settings)
    {
        if (settings == null) return false;

        if (string.IsNullOrWhiteSpace(settings.Endpoint)) return false;
        //if (string.IsNullOrWhiteSpace(settings.PredictionId)) return false;
        if (string.IsNullOrWhiteSpace(settings.PredictionKey)) return false;
        if (string.IsNullOrWhiteSpace(settings.ModelId)) return false;
        return true;
    }
}
