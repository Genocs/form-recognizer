namespace Genocs.Integration.CognitiveServices.Options;

/// <summary>
/// Azure blob storage settings 
/// </summary>
public class AzureStorageSettings
{
    /// <summary>
    /// Default section name
    /// </summary>
    public const string Position = "AzureStorage";

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
    public string? TrainingSetContainer { get; set; }

    /// <summary>
    /// The thumbnail container name
    /// </summary>
    public string? ThumbnailContainer { get; set; }

    /// <summary>
    /// Helper function to validate data
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static bool IsValid(AzureStorageSettings settings)
    {
        if (settings == null) return false;
        if (string.IsNullOrWhiteSpace(settings.AccountName)) return false;
        if (string.IsNullOrWhiteSpace(settings.AccountKey)) return false;
        if (string.IsNullOrWhiteSpace(settings.UploadContainer)) return false;

        return true;
    }
}
