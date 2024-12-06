using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class InternalTransactionResponse : BaseResponse
    {
        public decimal Value { get; set; }

        public Guid ClientId { get; set; }

        public InternalTransactionType InternalTransactionType { get; set; }
    }
}
