namespace Genocs.Integration.CognitiveServices.Options;

/// <summary>
/// The ImageClassifierSettings object.
/// </summary>
public class ImageClassifierSettings
{
    /// <summary>
    /// Default Section name.
    /// </summary>
    public const string Position = "ImageClassifier";

    /// <summary>
    /// The cognitive service root endpoint.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The subscription key.
    /// </summary>
    public string? SubscriptionKey { get; set; }

    /// <summary>
    /// The model id.
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Static helper function used to validate the settings.
    /// </summary>
    /// <param name="settings">Object to validate.</param>
    /// <returns>true in case is OK otherwise false.</returns>
    public static bool IsValid(ImageClassifierSettings settings)
    {
        if (settings == null) return false;
        if (string.IsNullOrWhiteSpace(settings.Endpoint)) return false;
        if (string.IsNullOrWhiteSpace(settings.SubscriptionKey)) return false;
        if (string.IsNullOrWhiteSpace(settings.ModelId)) return false;
        return true;
    }
}
