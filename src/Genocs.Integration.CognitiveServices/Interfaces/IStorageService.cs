namespace Genocs.Integration.CognitiveServices.Interfaces;


/// <summary>
/// The storage service interface
/// </summary>
public interface IStorageService
{
    Task<string?> UploadImageAsync(string blobName, Stream file);
    string GetBlobUriWithSASToken(string blobName);
}
