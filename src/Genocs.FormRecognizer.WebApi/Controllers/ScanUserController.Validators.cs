using FluentValidation;

namespace Genocs.FormRecognizer.WebApi.Controllers;


public class MemberScanRequestValidator : AbstractValidator<MemberScanRequest>
{
    public MemberScanRequestValidator()
    {
        RuleFor(c => c.FaceImageUrl).NotNull();
        RuleFor(c => c.IdDocumentImageUrl).NotNull();
    }
}