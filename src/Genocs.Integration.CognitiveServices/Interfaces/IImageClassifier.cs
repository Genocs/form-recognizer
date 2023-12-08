using Genocs.Integration.CognitiveServices.Contracts;

namespace Genocs.Integration.CognitiveServices.Interfaces;


/// <summary>
/// Interface for image classifier.
/// </summary>
public interface IImageClassifier
{
    /// <summary>
    /// Classify the image against a custom model 
    /// </summary>
    /// <param name="url">The image absolute url</param>
    /// <returns></returns>
    Task<Classification?> ClassifyAsync(string url);
}
