namespace Genocs.Integration.CognitiveServices.Interfaces;

/// <summary>
/// 
/// </summary>
public interface IFormRecognizer
{
    Task<List<dynamic>> ScanLocal(string classificationKey, string filePath);
    Task<List<dynamic>> ScanRemote(string classificationKey, string url);

}
