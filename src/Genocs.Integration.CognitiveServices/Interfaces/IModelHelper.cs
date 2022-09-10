namespace Genocs.Integration.CognitiveServices.Interfaces;

public interface IModelHelper
{
    /// <summary>
    /// Print info about the existing models
    /// </summary>
    public void EvaluateExisting();

    /// <summary>
    /// It allows t create a new model starting from the trainingsetUrl
    /// </summary>
    /// <param name="trainingSetUrl">The training set url</param>
    /// <param name="modelName">The model name</param>
    /// <returns></returns>
    Task CreateModelAsync(string trainingSetUrl, string modelName);
}
