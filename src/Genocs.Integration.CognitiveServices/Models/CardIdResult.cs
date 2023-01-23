using Genocs.Integration.CognitiveServices.Services;

namespace Genocs.Integration.CognitiveServices.Models;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
public class BaseTypeText
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}

public class Word
{
    public string? Text { get; set; }
    public List<int>? BoundingBox { get; set; }
    public double Confidence { get; set; }
}

public class Style
{
    public string Name { get; set; }
    public double Confidence { get; set; }
}

public class Appearance
{
    public Style style { get; set; }
}

public class Line
{
    public string text { get; set; }
    public List<int> boundingBox { get; set; }
    public List<Word> words { get; set; }
    public Appearance appearance { get; set; }
}

public class ReadResult
{
    public int page { get; set; }
    public double angle { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public string unit { get; set; }
    public List<Line> lines { get; set; }
}

public class Country : BaseTypeText
{
    public string? ValueCountry { get; set; }
}

public class DateValue : BaseTypeText
{
    public string? ValueDate { get; set; }
}


public class StringValue : BaseTypeText
{
    public string? ValueString { get; set; }
}


public class Sex : BaseTypeText
{
    public string? ValueGender { get; set; }
}

public class ValueObject
{
    public Country? Country { get; set; }
    public DateValue? DateOfBirth { get; set; }
    public DateValue? DateOfExpiration { get; set; }
    public StringValue? DocumentNumber { get; set; }
    public StringValue? FirstName { get; set; }
    public StringValue? LastName { get; set; }
    public Country? Nationality { get; set; }
    public Sex? Sex { get; set; }
}

public class MachineReadableZone : BaseTypeText
{
    public ValueObject? ValueObject { get; set; }
    public List<double>? BoundingBox { get; set; }
    public int Page { get; set; }
    public double Confidence { get; set; }
    public List<string>? Elements { get; set; }
}

public class Fields
{
    public MachineReadableZone? MachineReadableZone { get; set; }
}

public class DocumentResult
{
    public string? DocType { get; set; }
    public double DocTypeConfidence { get; set; }
    public List<int>? PageRange { get; set; }
    public Fields? Fields { get; set; }
}

public class AnalyzeResult
{
    public string? Version { get; set; }
    public List<ReadResult>? ReadResults { get; set; }
    public List<DocumentResult>? DocumentResults { get; set; }
}

public class CardIdResult
{
    public string? Status { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime LastUpdatedDateTime { get; set; }
    public AnalyzeResult? AnalyzeResult { get; set; }
}


/// <summary>
/// The ID Result
/// </summary>
public class IDResult
{

    /// <summary>
    /// The validation result
    /// </summary>
    public IDValidationResultType ValidationResult { get; set; }

    /// <summary>
    /// The mrz
    /// </summary>
    public string? MRZ { get; set; }

    /// <summary>
    /// The Data
    /// </summary>
    public IdentityDocumentData? Data { get; set; }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="validationResult"></param>
    public IDResult(IDValidationResultType validationResult)
    {
        ValidationResult = validationResult;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="validationResult"></param>
    /// <param name="mrz"></param>
    /// <param name="data"></param>
    public IDResult(IDValidationResultType validationResult, string mrz, IdentityDocumentData data)
    {
        ValidationResult = validationResult;
        MRZ = mrz;
        Data = data;
    }
}