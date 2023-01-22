using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Genocs.Integration.CognitiveServices.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// Storage service used to store images to Azure blob storage
/// </summary>
public class StorageService
{
    private readonly ILogger<StorageService> _logger;
    private readonly AzureStorageSettings _storageConfig;

    /// <summary>
    /// Standard constructor
    /// </summary>
    /// <param name="config">the config options</param>
    /// <param name="logger">The logger</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StorageService(IOptions<AzureStorageSettings> config, ILogger<StorageService> logger)
    {
        _ = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _storageConfig = config.Value;
    }


    private async Task<string> UploadFileToStorage(Stream fileStream, string fileName)
    {
        // Create a URI to the blob

        string url = $"https://{_storageConfig.AccountName}.blob.core.windows.net/{_storageConfig.UploadContainer}/{fileName}";
        Uri blobUri = new Uri(url);

        // Create StorageSharedKeyCredentials object by reading
        // the values from the configuration (appsettings.json)
        StorageSharedKeyCredential storageCredentials = new(_storageConfig.AccountName, _storageConfig.AccountKey);

        // Create the blob client.
        BlobClient blobClient = new(blobUri, storageCredentials);

        // Upload the file
        await blobClient.UploadAsync(fileStream, overwrite: true);

        string tkn = SASToken(_storageConfig.AccountName, _storageConfig.AccountKey, _storageConfig.UploadContainer, fileName);

        return tkn;
    }

    private async Task<List<string>> GetThumbNailUrls()
    {
        List<string> thumbnailUrls = new();

        // Create a URI to the storage account
        Uri accountUri = new Uri($"https://{_storageConfig.AccountName}.blob.core.windows.net/");

        // Create BlobServiceClient from the account URI
        BlobServiceClient blobServiceClient = new BlobServiceClient(accountUri);

        // Get reference to the container
        BlobContainerClient container = blobServiceClient.GetBlobContainerClient(_storageConfig.ThumbnailContainer);

        if (container.Exists())
        {
            foreach (BlobItem blobItem in container.GetBlobs())
            {
                thumbnailUrls.Add(container.Uri + "/" + blobItem.Name);
            }
        }
        else
        {
            _logger.LogWarning($"ThumbnailContainer: '{_storageConfig.ThumbnailContainer}' do not exist!");
        }

        return await Task.FromResult(thumbnailUrls);
    }

    /// <summary>
    /// Upload file to the storage
    /// </summary>
    /// <param name="files">IFormFile list of files</param>
    /// <returns>Liast of uploaded object</returns>
    public async Task<List<UploadedItem>> UploadFilesAsync(List<IFormFile> files)
    {
        List<UploadedItem> result = new();

        if (files is null || !files.Any())
        {
            return result;
        }

        foreach (var formFile in files)
        {
            if (ImageFormatHelper.IsImage(formFile))
            {
                if (formFile.Length > 0)
                {
                    using Stream stream = formFile.OpenReadStream();

                    // formFile.FileName could be changed to guid to generate unique id for storage
                    string uploadingFile = formFile.FileName;
                    //uploadingFile = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
                    string url = await UploadFileToStorage(stream, uploadingFile);
                    result.Add(new UploadedItem() { FileName = formFile.FileName, URL = url });
                }
            }
        }
        return result;
    }


    /// <summary>
    /// This function allows to add a query containing the SAS token to access private resources
    /// </summary>
    /// <param name="accountName">The storage account name</param>
    /// <param name="accountKey">The storage account key</param>
    /// <param name="containerName">The container name</param>
    /// <param name="blobName">The blob name</param>
    /// <returns></returns>
    private static string SASToken(string accountName, string accountKey, string containerName, string blobName)
    {
        BlobSasBuilder sasBuilder = new()
        {
            ExpiresOn = DateTime.UtcNow + (new TimeSpan(0, 15, 0)),
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b" // Generate the token for a blob
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // Build the SAS Token
        string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, accountKey)).ToString();

        UriBuilder uriBuilder = new UriBuilder()
        {
            Scheme = "https",
            Host = string.Format("{0}.blob.core.windows.net", accountName),
            Path = string.Format("{0}/{1}", containerName, blobName),
            Query = sasToken
        };

        // Return the full URI
        return uriBuilder.Uri.ToString();
    }
}

public class UploadedItem
{
    public string FileName { get; set; } = default!;
    public string URL { get; set; } = default!;
}
