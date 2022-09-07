using System.Collections.Generic;

namespace Genocs.Integration.ML.CognitiveServices.Models;

public class FormExtractorResult
{
    public string? ResourceUrl { get; set; }
    public Classification? Classification { get; set; }
    public List<dynamic>? ContentData { get; set; }
}
