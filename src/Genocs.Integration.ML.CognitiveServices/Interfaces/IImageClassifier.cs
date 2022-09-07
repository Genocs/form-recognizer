using Genocs.Integration.ML.CognitiveServices.Models;

namespace Genocs.Integration.ML.CognitiveServices.Interfaces;

public interface IImageClassifier
{
    Task<Classification> Classify(string url);
}
