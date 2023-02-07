namespace Genocs.FormRecognizer.WebApi.Models;

public class BasicRequest
{
    public string Url { get; set; } = default!;
}

public class EvaluateRequest : BasicRequest
{
    public string ClassificationModelId { get; set; } = default!;
}

public class SetupSettingRequest
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}
