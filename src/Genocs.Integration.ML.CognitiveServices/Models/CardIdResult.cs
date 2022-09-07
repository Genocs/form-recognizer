namespace Genocs.Integration.ML.CognitiveServices.Models;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
public class Word
{
    public string text { get; set; }
    public List<int> boundingBox { get; set; }
    public double confidence { get; set; }
}

public class Style
{
    public string name { get; set; }
    public double confidence { get; set; }
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

public class Country
{
    public string type { get; set; }
    public string text { get; set; }
    public string valueCountry { get; set; }
}

public class DateOfBirth
{
    public string type { get; set; }
    public string valueDate { get; set; }
    public string text { get; set; }
}

public class DateOfExpiration
{
    public string type { get; set; }
    public string valueDate { get; set; }
    public string text { get; set; }
}

public class DocumentNumber
{
    public string type { get; set; }
    public string valueString { get; set; }
    public string text { get; set; }
}

public class FirstName
{
    public string type { get; set; }
    public string valueString { get; set; }
    public string text { get; set; }
}

public class LastName
{
    public string type { get; set; }
    public string valueString { get; set; }
    public string text { get; set; }
}

public class Nationality
{
    public string type { get; set; }
    public string text { get; set; }
    public string valueCountry { get; set; }
}

public class Sex
{
    public string type { get; set; }
    public string text { get; set; }
    public string valueGender { get; set; }
}

public class ValueObject
{
    public Country Country { get; set; }
    public DateOfBirth DateOfBirth { get; set; }
    public DateOfExpiration DateOfExpiration { get; set; }
    public DocumentNumber DocumentNumber { get; set; }
    public FirstName FirstName { get; set; }
    public LastName LastName { get; set; }
    public Nationality Nationality { get; set; }
    public Sex Sex { get; set; }
}

public class MachineReadableZone
{
    public string type { get; set; }
    public ValueObject valueObject { get; set; }
    public string text { get; set; }
    public List<double> boundingBox { get; set; }
    public int page { get; set; }
    public double confidence { get; set; }
    public List<string> elements { get; set; }
}

public class Fields
{
    public MachineReadableZone MachineReadableZone { get; set; }
}

public class DocumentResult
{
    public string docType { get; set; }
    public double docTypeConfidence { get; set; }
    public List<int> pageRange { get; set; }
    public Fields fields { get; set; }
}

public class AnalyzeResult
{
    public string version { get; set; }
    public List<ReadResult> readResults { get; set; }
    public List<DocumentResult> DocumentResults { get; set; }
}

public class CardIdResult
{
    public string Status { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime LastUpdatedDateTime { get; set; }
    public AnalyzeResult AnalyzeResult { get; set; }
}
