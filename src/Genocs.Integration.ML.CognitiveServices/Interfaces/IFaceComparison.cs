using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genocs.Integration.ML.CognitiveServices.Interfaces
{
    public interface IFaceComparison
    {
        Task<IList<SimilarFace>> FindSimilar(string firstImage, string secondImage);
    }
}
