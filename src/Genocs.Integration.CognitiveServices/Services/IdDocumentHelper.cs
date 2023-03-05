using Azure.AI.FormRecognizer.DocumentAnalysis;
using Genocs.Integration.CognitiveServices.Models;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// 
/// </summary>
public static class IdDocumentHelper
{
    /// <summary>
    /// The model ID
    /// </summary>
    public const string MODEL_ID = "prebuilt-idDocument";

    /// <summary>
    /// The document type
    /// </summary>
    public const string DOCUMENT_TYPE = "idDocument.passport";

    private const float ConfidenceThreshold = 0.85f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="document"></param>
    public static IDResult Validate(AnalyzedDocument? document)
    {
        if (document is null)
        {
            return new IDResult(IDValidationResultType.EMPTY_DATA);
        }

        if (document.DocumentType != DOCUMENT_TYPE)
        {
            return new IDResult(IDValidationResultType.NO_ID);
        }

        if (document.Confidence < ConfidenceThreshold)
        {
            return new IDResult(IDValidationResultType.UNDER_THRESHOLD);
        }

        if (document.BoundingRegions != null && !document.BoundingRegions.Any())
        {
            return new IDResult(IDValidationResultType.NO_IMAGE_BOUND);
        }

        string? mrz = IdDocumentHelper.GetContentString(document.Fields["MachineReadableZone"]);
        if (string.IsNullOrWhiteSpace(mrz))
        {
            return new IDResult(IDValidationResultType.MISSING_MRZ);
        }

        IDDocument? doc = MrzHelper.ExtractMRZ(mrz);

        if (doc is null)
        {
            return new IDResult(IDValidationResultType.INCONSISTENT_MRZ);
        }

        IdentityDocumentData iDResult = new IdentityDocumentData((document.Fields.ContainsKey("DocumentType") ? GetContentString(document.Fields["DocumentType"]) : null) ?? doc.DocumentType,
                                                                    doc.EmissionCountry,
                                                                    (document.Fields.ContainsKey("LastName") ? GetContentString(document.Fields["LastName"]) : null) ?? doc.PrimaryName,
                                                                    (document.Fields.ContainsKey("FirstName") ? GetContentString(document.Fields["FirstName"]) : null) ?? doc.SecondaryName,
                                                                    (document.Fields.ContainsKey("DocumentNumber") ? GetContentString(document.Fields["DocumentNumber"]) : null) ?? doc.DocumentNumber,
                                                                    doc.Nationality,
                                                                    doc.DateOfBirth,
                                                                    doc.Sex,
                                                                    doc.ExpirationDate,
                                                                    (document.Fields.ContainsKey("PersonalNumber") ? GetContentString(document.Fields["PersonalNumber"]) : null) ?? doc.PersonalNumber);

        mrz = MrzHelper.RemoveWhitespace(mrz);
        mrz = MrzHelper.RemoveWhitespace(mrz);
        return new IDResult(IDValidationResultType.VALID, mrz, iDResult);
    }

    /// <summary>
    /// Extract String
    /// </summary>
    /// <param name="field">The field</param>
    /// <returns></returns>
    public static string? GetContentString(DocumentField field)
    {
        if (field is null) return null;
        if (field.Confidence < .50f) return null;

        return field.Content;
    }
}

/// <summary>
/// ID result
/// </summary>
public enum IDValidationResultType
{
    /// <summary>
    /// No data to evaluate
    /// </summary>
    EMPTY_DATA,

    /// <summary>
    /// Id not found
    /// </summary>
    NO_ID,

    /// <summary>
    /// Confidence under threshold
    /// </summary>
    UNDER_THRESHOLD,

    /// <summary>
    /// NO bouding region found
    /// </summary>
    NO_IMAGE_BOUND,

    /// <summary>
    /// The MRZ Data is missing
    /// </summary>
    MISSING_MRZ,

    /// <summary>
    /// The MRZ Data is inconsistent
    /// </summary>
    INCONSISTENT_MRZ,

    /// <summary>
    /// NO issue
    /// </summary>
    VALID
}
