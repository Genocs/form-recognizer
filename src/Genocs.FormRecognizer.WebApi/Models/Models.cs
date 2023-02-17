namespace Genocs.FormRecognizer.WebApi.Models;

public class BasicRequest
{
    public string Url { get; set; } = default!;
}

public class EvaluateRequest : BasicRequest
{
    public string ClassificationModelId { get; set; } = default!;
}
