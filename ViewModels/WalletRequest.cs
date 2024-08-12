using Microsoft.EntityFrameworkCore;

namespace SFManagement.ViewModels
{
    public class WalletRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid ManagerId { get; set; }

        public decimal IntialCredits { get; set; }

        public decimal IntialBalance { get; set; }

        public decimal InitialRate { get; set; }
    }
}
