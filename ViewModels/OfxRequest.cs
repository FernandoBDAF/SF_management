namespace SFManagement.ViewModels;

public class OfxRequest
{
    public string? FileName { get; set; }

    public IFormFile PostFile { get; set; }

    public Guid BankId { get; set; }
}