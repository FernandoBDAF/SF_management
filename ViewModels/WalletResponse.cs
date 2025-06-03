namespace SFManagement.ViewModels
{
    public class WalletResponse : BaseResponse
    {
        public string Name { get; set; }
        
        public Guid ManagerId { get; set; }

        public decimal InitialCoins { get; set; }

        public decimal InitialValue { get; set; }

        public decimal InitialExchangeRate { get; set; }
    }
}
