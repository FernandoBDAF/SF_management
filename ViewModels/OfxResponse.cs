using SFManagement.Models;

namespace SFManagement.ViewModels
{
    public class OfxResponse : BaseResponse
    {
        public Guid BankId { get; set; }

        public string? FileName { get; set; }

        public List<BankTransactionResponse>? BankTransactions { get; set; } = new List<BankTransactionResponse>();
        
        public BankResponse Bank { get; set; }
    }
}
