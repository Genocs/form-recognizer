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
    public static IDValidationResultTypes Validate(Azure.AI.FormRecognizer.DocumentAnalysis.AnalyzedDocument? document)
    {
        IDValidationResultTypes result = IDValidationResultTypes.EMPTY_DATA;
        if (document is null)
        {
            return result;
        }

        if (document.DocumentType != DOCUMENT_TYPE)
        {
            result = IDValidationResultTypes.NO_ID;
            return result;
        }

        if (document.Confidence < ConfidenceThreshold)
        {
            result = IDValidationResultTypes.UNDER_THRESHOLD;
            return result;
        }

        if (document.BoundingRegions != null && !document.BoundingRegions.Any())
        {
            result = IDValidationResultTypes.NO_IMAGE_BOUND;
            return result;
        }


        // Check Width and Height
        var region = document.BoundingRegions.FirstOrDefault();


        result = IDValidationResultTypes.VALID;

        return result;
    }
}

/// <summary>
/// ID result
/// </summary>
public enum IDValidationResultTypes
{
    /// <summary>
    /// No data to evalaute
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
    /// NO issue
    /// </summary>
    VALID
}
