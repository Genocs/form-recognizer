namespace Genocs.Integration.CognitiveServices.Interfaces;

public interface IFormRecognizer
{
    Task<List<dynamic>> ScanLocal(string classificationKey, string filePath);
    Task<List<dynamic>> ScanRemote(string classificationKey, string url);
    public Task<string?> ScanLocalCardId(string filePath);
    public Task<string?> ScanRemoteCardId(string url);

}
