using SFManagement.Application.DTOs.Transactions;

namespace SFManagement.Domain.Exceptions;

public class WalletMissingException : Exception
{
    public WalletMissingError Details { get; }

    public WalletMissingException(WalletMissingError details)
        : base(details.Message)
    {
        Details = details;
    }
}

