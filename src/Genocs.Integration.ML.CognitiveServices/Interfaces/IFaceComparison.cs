using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Genocs.Integration.ML.CognitiveServices.Interfaces;

public interface IFaceComparison
{
    Task<IList<SimilarFace>> FindSimilar(string firstImage, string secondImage);
}
