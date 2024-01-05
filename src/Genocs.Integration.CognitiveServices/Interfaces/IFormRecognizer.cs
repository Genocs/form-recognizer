namespace Genocs.Integration.CognitiveServices.Interfaces;

/// <summary>
/// Interface for form recognizer.
/// </summary>
public interface IFormRecognizer
{
    /// <summary>
    /// Scan async a document as stream.
    /// </summary>
    /// <param name="classificationKey">The classification key.</param>
    /// <param name="stream">The stream.</param>
    /// <returns></returns>
    Task<List<dynamic>> ScanAsync(string classificationKey, Stream stream);

    /// <summary>
    /// Scan async a document stored on a blob storage.
    /// </summary>
    /// <param name="classificationKey">The classification key.</param>
    /// <param name="url">The document url.</param>
    /// <returns></returns>
    Task<List<dynamic>> ScanAsync(string classificationKey, string url);
}
