namespace SFManagement.ViewModels
{
    public class BankResponse : BaseResponse
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public decimal InitialValue { get; set; }
    }
}
