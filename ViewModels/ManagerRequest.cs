using Microsoft.EntityFrameworkCore;

namespace SFManagement.ViewModels
{
    public class ManagerRequest
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        [Precision(18, 2)]
        public decimal IntialCredits { get; set; }

        [Precision(18, 2)]
        public decimal IntialBalance { get; set; }

        [Precision(18, 2)]
        public decimal InitialRate { get; set; }
    }
}