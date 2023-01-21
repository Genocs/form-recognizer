namespace Genocs.Integration.CognitiveServices.Options;

/// <summary>
/// Azure blob storage settings 
/// </summary>
public class AzureStorageSettings
{
    /// <summary>
    /// Default section name
    /// </summary>
    public const string Position = "AzureStorageSettings";

    /// <summary>
    /// The storage account name
    /// </summary>
    public string AccountName { get; set; } = default!;

    /// <summary>
    /// The storage account key
    /// </summary>
    public string AccountKey { get; set; } = default!;

    /// <summary>
    /// the root storage container name
    /// </summary>
    public string UploadContainer { get; set; } = default!;

    /// <summary>
    /// The trainingset container name
    /// </summary>
    public string TrainingSetContainer { get; set; } = default!;

    /// <summary>
    /// The thumbnail container name
    /// </summary>
    public string ThumbnailContainer { get; set; } = default!;
}
