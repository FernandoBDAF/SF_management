namespace SFManagement.ViewModels
{
    public class ExcelRequest
    {
        public Guid WalletId { get; set; }

        public IFormFile PostFile { get; set; }
    }
}
