namespace Genocs.Integration.CognitiveServices.Options;

public class AzureCognitiveServicesSettings
{
    /// <summary>
    /// Default Section name
    /// </summary>
    public static string Position = "AzureCognitiveServicesSettings";

    public string Endpoint { get; set; } = default!;
    public string SubscriptionKey { get; set; } = default!;
}
