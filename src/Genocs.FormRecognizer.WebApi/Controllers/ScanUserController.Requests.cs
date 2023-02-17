namespace Genocs.FormRecognizer.WebApi.Controllers;

public class MemberScanRequest
{
    public string IdDocumentImageUrl { get; set; } = default!;
    public string FaceImageUrl { get; set; } = default!;
}
