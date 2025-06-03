using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class ManagerRequest
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public ManagerType ManagerType { get; set; }

        public decimal InitialValue { get; set; }

        public decimal InitialExchangeRate { get; set; }

        public decimal InitialCoins { get; set; }
    }
}