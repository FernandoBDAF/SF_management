using SFManagement.Models.Entities;
using SFManagement.ViewModels;
using SFManagement.Enums;
using SFManagement.Exceptions;

namespace SFManagement.Interfaces;

/// <summary>
/// Domain service interface for asset holder business logic
/// </summary>
public interface IAssetHolderDomainService
{
    /// <summary>
    /// Validates if an asset holder can be deleted based on business rules
    /// </summary>
    Task<bool> CanDeleteAssetHolder(Guid assetHolderId);
    
    /// <summary>
    /// Validates asset holder creation request with comprehensive business rules
    /// </summary>
    Task<DomainValidationResult> ValidateAssetHolderCreation(BaseAssetHolderRequest request);
    
    /// <summary>
    /// Determines the asset holder type for a given ID
    /// </summary>
    Task<AssetHolderType> DetermineAssetHolderType(Guid assetHolderId);
    
    /// <summary>
    /// Checks if an asset holder has any active transactions
    /// </summary>
    Task<bool> HasActiveTransactions(Guid assetHolderId);
    
    /// <summary>
    /// Calculates the total balance across all asset types for an asset holder
    /// </summary>
    Task<decimal> GetTotalBalance(Guid assetHolderId);
    
    /// <summary>
    /// Validates business rules for specific entity types
    /// </summary>
    Task<DomainValidationResult> ValidateClientCreation(ClientRequest request);
    Task<DomainValidationResult> ValidateBankCreation(BankRequest request);
    Task<DomainValidationResult> ValidateMemberCreation(MemberRequest request);
    Task<DomainValidationResult> ValidatePokerManagerCreation(PokerManagerRequest request);
    
    /// <summary>
    /// Checks if an asset holder has any active asset wallets
    /// </summary>
    Task<bool> HasActiveAssetPools(Guid assetHolderId);
    
    /// <summary>
    /// Validates that CPF/CNPJ are unique across the system
    /// </summary>
    Task<bool> IsCpfUnique(string cpf, Guid? excludeAssetHolderId = null);
    Task<bool> IsCnpjUnique(string cnpj, Guid? excludeAssetHolderId = null);
    Task<bool> IsEmailUnique(string email, Guid? excludeAssetHolderId = null);
    
    /// <summary>
    /// Validates bank code uniqueness
    /// </summary>
    Task<bool> IsBankCodeUnique(string code, Guid? excludeBankId = null);
}

/// <summary>
/// Domain validation result with detailed error information
/// </summary>
public class DomainValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<ValidationError> Errors { get; set; } = new();
    
    public void AddError(string field, string message, string? code = null)
    {
        IsValid = false;
        Errors.Add(new ValidationError(field, message, code));
    }
    
    public void AddError(ValidationError error)
    {
        IsValid = false;
        Errors.Add(error);
    }
} 