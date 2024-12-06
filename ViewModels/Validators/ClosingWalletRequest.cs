using FluentValidation;
using SFManagement.Models;

namespace SFManagement.ViewModels.Validators
{
    public class ClosingWalletRequest : AbstractValidator<ClosingWallet>
    {
        public ClosingWalletRequest()
        {
        }
    }
}
