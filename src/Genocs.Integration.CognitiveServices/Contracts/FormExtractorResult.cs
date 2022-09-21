namespace Genocs.Integration.CognitiveServices.Contracts;

public class FormExtractorResult
{
    public string? ResourceUrl { get; set; }
    public Classification? Classification { get; set; }
    public List<dynamic>? ContentData { get; set; }
}
