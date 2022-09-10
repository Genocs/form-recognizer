namespace Genocs.Integration.ML.CognitiveServices.Options;

public class AzureStorageConfig
{
    public const string Position = "AzureStorageConfig";
    public string AccountName { get; set; } = default!;
    public string AccountKey { get; set; } = default!;
    public string UploadContainer { get; set; } = default!;
    public string TrainingSetContainerUrl { get; set; } = default!;
    public string ThumbnailContainer { get; set; } = default!;
    public string InspectingFileUrl { get; set; } = default!;
}
