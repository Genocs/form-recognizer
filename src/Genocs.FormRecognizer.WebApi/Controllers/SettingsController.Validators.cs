using FluentValidation;
namespace Genocs.FormRecognizer.WebApi.Controllers;

public class SetupSettingRequestValidator : AbstractValidator<SetupSettingRequest>
{
    public SetupSettingRequestValidator()
    {
        RuleFor(c => c.Key).NotNull();
        RuleFor(c => c.Value).NotNull();
    }
}
