using Genocs.Integration.CognitiveServices.IntegrationEvents;

namespace Genocs.FormRecognizer.WebApi.Models;

/// <summary>
/// The basic request.
/// </summary>
public class BasicRequest
{
    /// <summary>
    /// The unique RequestId.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// The unique ContextId.
    /// </summary>
    public string? ContextId { get; set; }

    /// <summary>
    /// ReferenceId is used to correlate the request with external reference.
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Resource url used to extract the data.
    /// </summary>
    public string Url { get; set; } = default!;
}

public class EvaluateRequest : BasicRequest
{
    public string ClassificationModelId { get; set; } = default!;
}

public class FormExtractorResponse : FormDataExtractionCompleted
{

}