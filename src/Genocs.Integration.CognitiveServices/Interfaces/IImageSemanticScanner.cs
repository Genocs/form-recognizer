namespace Genocs.Integration.CognitiveServices.Interfaces;

/// <summary>
/// Interface for generic image semantic scanner.
/// </summary>
public interface IImageSemanticScanner
{
    /// <summary>
    /// Scan async a document stored on a blob storage.
    /// </summary>
    /// <param name="url">The public accessible document url.</param>
    /// <returns></returns>
    Task<List<dynamic>> ScanAsync(string url);
}
