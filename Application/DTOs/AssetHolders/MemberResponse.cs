using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.Support;
using SFManagement.Domain.Entities.AssetHolders;

namespace SFManagement.Application.DTOs.AssetHolders;

public class MemberResponse : BaseAssetHolderResponse
{
    public DateTime? Birthday { get; set; }
    public decimal? Share { get; set; }

    public decimal? Salary { get; set; }
    
    /// <summary>
    /// Calculated age based on birthday
    /// </summary>
    public int? Age => Birthday.HasValue ? 
        DateTime.Now.Year - Birthday.Value.Year - 
        (DateTime.Now.DayOfYear < Birthday.Value.DayOfYear ? 1 : 0) : null;
    
    /// <summary>
    /// Indicates if the member has an active share (greater than 0)
    /// </summary>
    public bool IsActiveShare => Share.HasValue && Share.Value > 0;
    
    // Remove redundant collections - these should be accessed through separate endpoints
    // Wallets, WalletIdentifiers, InitialBalances, ContactPhones create circular references
    // and performance issues in responses. Use dedicated endpoints instead.
}