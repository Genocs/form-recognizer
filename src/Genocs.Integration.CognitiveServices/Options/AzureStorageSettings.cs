namespace Genocs.Integration.CognitiveServices.Options;

public class AzureStorageSettings
{
    /// <summary>
    /// Default Section name
    /// </summary>
    public const string Position = "AzureStorageSettings";
    public string AccountName { get; set; } = default!;
    public string AccountKey { get; set; } = default!;
    public string UploadContainer { get; set; } = default!;
    public string TrainingSetContainerUrl { get; set; } = default!;
    public string ThumbnailContainer { get; set; } = default!;
    public string InspectingFileUrl { get; set; } = default!;
}
