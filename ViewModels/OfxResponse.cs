namespace SFManagement.ViewModels
{
    public class OfxResponse : BaseResponse
    {
        public Guid BankId { get; set; }

        public string BankName { get; set; }

        public string? FileName { get; set; }
    }
}
