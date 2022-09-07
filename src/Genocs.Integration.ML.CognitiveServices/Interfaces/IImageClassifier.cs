using Genocs.Integration.ML.CognitiveServices.Models;
using System.Threading.Tasks;

namespace Genocs.Integration.ML.CognitiveServices.Interfaces;

public interface IImageClassifier
{
    Task<Classification> Classify(string url);
}
