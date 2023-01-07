namespace Genocs.Integration.CognitiveServices.Contracts;

/// <summary>
/// The FormExtractorResponse data response
/// </summary>
public class FormExtractorResponse
{
    /// <summary>
    /// The processed resource url
    /// </summary>
    public string ResourceUrl { get; set; } = default!;

    /// <summary>
    /// The classification object
    /// </summary>
    public Classification? Classification { get; set; }

    /// <summary>
    /// The dynamic data about the results
    /// </summary>
    public List<dynamic>? ContentData { get; set; }
}
