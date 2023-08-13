namespace Genocs.Integration.CognitiveServices.Contracts;

/// <summary>
/// The prediction result
/// </summary>
public class Classification
{
    /// <summary>
    /// The id of the prediction
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The project id
    /// </summary>
    public string? Project { get; set; }

    /// <summary>
    /// The iteration id
    /// </summary>
    public string? Iteration { get; set; }

    /// <summary>
    /// The created date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The predictions
    /// </summary>
    public List<Prediction>? Predictions { get; set; }
}
