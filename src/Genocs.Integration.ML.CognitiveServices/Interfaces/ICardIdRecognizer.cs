using Genocs.Integration.ML.CognitiveServices.Models;
using System.Threading.Tasks;

namespace Genocs.Integration.ML.CognitiveServices.Interfaces;

/// <summary>
/// The generic interface for Document Id Identifier
/// </summary>
public interface ICardIdRecognizer
{
    Task<CardIdResult> Recognize(string url);
}
