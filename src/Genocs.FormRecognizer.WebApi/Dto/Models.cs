namespace Genocs.FormRecognizer.WebApi.Dto;

public class BasicRequest
{
    public string Url { get; set; }
}

public class EvaluateRequest : BasicRequest
{
    public string ClassificationModelId { get; set; }
}

public class SetupSettingRequest
{
    public string Key { get; set; }
    public string Value { get; set; }

}
