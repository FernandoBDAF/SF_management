namespace SFManagement.ViewModels
{
    public class AvgRateResponse : BaseResponse
    {
        public Guid ManagerId { get; set; }

        public decimal Value { get; set; } = decimal.Zero;

        public DateTime Date { get; set; }
    }
}
