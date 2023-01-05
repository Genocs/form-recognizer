using Genocs.Integration.CognitiveServices.Models;

namespace Genocs.Integration.CognitiveServices.Interfaces;

/// <summary>
/// The generic interface for Document Id Identifier
/// </summary>
public interface ICardIdRecognizer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns>The result</returns>
    Task<CardIdResult?> Recognize(string url);
}
