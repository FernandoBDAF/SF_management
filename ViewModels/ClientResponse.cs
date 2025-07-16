using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class ClientResponse : BaseAssetHolderResponse
{
    public DateTime? Birthday { get; set; }
    
    /// <summary>
    /// Calculated age based on birthday
    /// </summary>
    public int? Age => Birthday.HasValue ? 
        DateTime.Now.Year - Birthday.Value.Year - 
        (DateTime.Now.DayOfYear < Birthday.Value.DayOfYear ? 1 : 0) : null;
    
    // Remove redundant collections - these should be accessed through separate endpoints
    // WalletIdentifiers, AssetPools, InitialBalances, ContactPhones create circular references
    // and performance issues in responses. Use dedicated endpoints instead.
}