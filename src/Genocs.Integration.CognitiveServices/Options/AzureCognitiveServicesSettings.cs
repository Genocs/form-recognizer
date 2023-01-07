namespace Genocs.Integration.CognitiveServices.Options;

/// <summary>
/// The AzureCognitiveServicesSettings object 
/// </summary>
public class AzureCognitiveServicesSettings
{
    /// <summary>
    /// Default Section name
    /// </summary>
    public static string Position = "AzureCognitiveServicesSettings";

    /// <summary>
    /// Azure Cognitive Services URL
    /// </summary>
    public string Endpoint { get; set; } = default!;

    /// <summary>
    /// The Azure Cognitive Services SubscriptionKey
    /// </summary>
    public string SubscriptionKey { get; set; } = default!;


    /// <summary>
    /// Validator
    /// </summary>
    /// <param name="settings">instance object to validate</param>
    /// <returns>true if valid otherwise false</returns>
    public static bool IsValid(AzureCognitiveServicesSettings settings)
    {
        if (settings is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.Endpoint) || string.IsNullOrWhiteSpace(settings.SubscriptionKey))
        {
            return false;
        }
        return true;
    }
}
