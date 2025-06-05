using FluentValidation;

namespace SFManagement.ViewModels.Validators;

public class ExcelRequestValidator : AbstractValidator<ExcelRequest>
{
    public ExcelRequestValidator()
    {
        // RuleFor(x => x.ManagerId).NotEmpty();
        //
        // RuleFor(x => x.PostFile).Must((file) =>
        // {
        //     var extension = Path.GetExtension(file.FileName).ToLower();
        //     return extension.Contains("xls") || extension.Contains("XLS") || extension.Contains("XLSX") || extension.Contains("xlsx");
        // }).WithMessage("Formato de arquivo inválido. Apenas .xls ou .xlsx são aceitos.");
        RuleFor(x => x.ManagerId).NotEmpty();

        RuleFor(x => x.PostFile).Must(file => { return file != null && file.Length > 0; })
            .WithMessage("File is required.");

        RuleFor(x => x.PostFile).Must(file =>
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            return extension.Contains("xls") || extension.Contains("XLS") || extension.Contains("XLSX") ||
                   extension.Contains("xlsx");
        }).WithMessage("Formato de arquivo inválido. Apenas .xls ou .xlsx são aceitos.");
    }
}