namespace Genocs.Integration.CognitiveServices.Options;

public class AzureCognitiveServicesConfig
{
    public const string Position = "AzureCognitiveServicesConfig";
    public string Endpoint { get; set; } = default!;
    public string SubscriptionKey { get; set; } = default!;
}
