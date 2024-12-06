namespace SFManagement.ViewModels
{
    public class ClosingNicknameRequest
    {
        public Guid NicknameId { get; set; }

        public Guid ClosingManagerId { get; set; }

        public decimal Balance { get; set; }

        public decimal Rake { get; set; }

        public decimal Rakeback { get; set; }

        public Guid? FatherNicknameId { get; set; }

        public decimal FatherPercentual { get; set; }
    }
}
