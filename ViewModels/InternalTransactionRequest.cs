using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class InternalTransactionRequest
    {
        public decimal Value { get; set; }

        public Guid ClientId { get; set; }

        public InternalTransactionType InternalTransactionType { get; set; }
    }
}
