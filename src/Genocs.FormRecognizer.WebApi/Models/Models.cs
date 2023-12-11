namespace Genocs.FormRecognizer.WebApi.Models;

public class BasicRequest
{
    public string? RequestId { get; set; }
    public string? ContextId { get; set; }
    public string Url { get; set; } = default!;
}

public class EvaluateRequest : BasicRequest
{
    public string ClassificationModelId { get; set; } = default!;
}
