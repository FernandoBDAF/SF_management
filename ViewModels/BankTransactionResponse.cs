namespace SFManagement.ViewModels
{
    public class BankTransactionResponse : BaseResponse
    {
        public Guid BankId { get; set; }

        public decimal Value { get; set; }

        public string? Description { get; set; }
    }
}
