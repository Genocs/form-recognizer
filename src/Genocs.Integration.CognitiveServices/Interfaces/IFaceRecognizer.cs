using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Genocs.Integration.CognitiveServices.Interfaces;


/// <summary>
/// Interface for face recognizer 
/// </summary>
public interface IFaceRecognizer
{
    /// <summary>
    /// Check face match
    /// </summary>
    /// <param name="firstImage">First image containing a face</param>
    /// <param name="secondImage">Second image containing a face</param>
    /// <returns>The result</returns>
    Task<IList<SimilarFace>> CompareFacesAsync(string firstImage, string secondImage);
}
