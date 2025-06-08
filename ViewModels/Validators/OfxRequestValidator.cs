using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class OfxRequestValidator : AbstractValidator<OfxRequest>
{
    public OfxRequestValidator()
    {
        RuleFor(x => x.BankId).NotEmpty();

        RuleFor(x => x.PostFile).Must(file => { return file != null && file.Length > 0; })
            .WithMessage("File is required.");

        RuleFor(x => x.PostFile).Must(file =>
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            return extension.Contains("OFX") || extension.Contains("ofx");
        }).WithMessage("Formato de arquivo inválido. Apenas .ofx são aceitos.");
    }
}