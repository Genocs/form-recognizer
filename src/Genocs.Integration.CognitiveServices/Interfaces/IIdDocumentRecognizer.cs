using Genocs.Integration.CognitiveServices.Models;

namespace Genocs.Integration.CognitiveServices.Interfaces;

/// <summary>
/// The generic interface for Document Id Recognizer
/// </summary>
public interface IIdDocumentRecognizer
{
    /// <summary>
    /// The actual function to extract data from ID document
    /// </summary>
    /// <param name="url">url to the resource, it could be a blob storage</param>
    /// <returns>The result</returns>
    Task<CardIdResult?> RecognizeAsync(string url);
}
