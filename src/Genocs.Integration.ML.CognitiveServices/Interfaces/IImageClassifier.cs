using Genocs.FormRecognizer.Contracts;

namespace Genocs.Integration.ML.CognitiveServices.Interfaces;

public interface IImageClassifier
{
    Task<Classification> Classify(string url);
}
