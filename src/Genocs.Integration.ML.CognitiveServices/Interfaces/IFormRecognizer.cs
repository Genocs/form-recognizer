namespace Genocs.Integration.ML.CognitiveServices.Interfaces;

public interface IFormRecognizer
{
    Task<List<dynamic>> ScanLocal(string classificationKey, string filePath);
    Task<List<dynamic>> ScanRemote(string classificationKey, string url);
    public Task ScanLocalCardId(string filePath);
    public Task ScanRemoteCardId(string url);
}
