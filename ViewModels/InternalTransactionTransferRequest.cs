namespace SFManagement.ViewModels
{
    public class InternalTransactionTransferRequest
    {
        public decimal Value { get; set; }

        public decimal? Coins { get; set; }

        public decimal? ExchangeRate { get; set; }
    }
}
