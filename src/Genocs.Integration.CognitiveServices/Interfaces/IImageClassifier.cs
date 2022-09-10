using Genocs.FormRecognizer.Contracts;

namespace Genocs.Integration.CognitiveServices.Interfaces;

public interface IImageClassifier
{
    Task<Classification?> Classify(string url);
}
