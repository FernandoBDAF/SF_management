namespace SFManagement.ViewModels
{
    public class OfxResponse : BaseResponse
    {
        public int BankId { get; set; }

        public string? FileName { get; set; }

        public byte[]? File { get; set; }
    }
}
