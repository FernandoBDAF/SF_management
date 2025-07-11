using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Exceptions;
using SFManagement.Interfaces;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;
using System.Text.RegularExpressions;

namespace SFManagement.Services;

/// <summary>
/// Domain service implementation for asset holder business logic
/// </summary>
public class AssetHolderDomainService : IAssetHolderDomainService
{
    private readonly DataContext _context;
    
    public AssetHolderDomainService(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> CanDeleteAssetHolder(Guid assetHolderId)
    {
        // Check for active transactions
        var hasActiveTransactions = await HasActiveTransactions(assetHolderId);
        if (hasActiveTransactions) return false;
        
        // Check for active asset wallets with balances
        var hasActiveAssetPools = await HasActiveAssetPools(assetHolderId);
        if (hasActiveAssetPools) return false;
        
        // Check for active referrals - commented out as Referrals table doesn't exist yet
        // var hasActiveReferrals = await _context.Referrals
        //     .AnyAsync(r => r.AssetHolderId == assetHolderId && 
        //               r.ActiveUntil > DateTime.UtcNow && 
        //               !r.DeletedAt.HasValue);
        // if (hasActiveReferrals) return false;
        
        return true;
    }

    public async Task<DomainValidationResult> ValidateAssetHolderCreation(BaseAssetHolderRequest request)
    {
        var result = new DomainValidationResult();
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            result.AddError("Name", "Name is required", "REQUIRED_FIELD");
        }
        
        // Validate name length and format
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            if (request.Name.Length < 2)
            {
                result.AddError("Name", "Name must be at least 2 characters long", "MIN_LENGTH");
            }
            
            if (request.Name.Length > 40)
            {
                result.AddError("Name", "Name cannot exceed 40 characters", "MAX_LENGTH");
            }
        }
        
        // Validate email format and uniqueness
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!IsValidEmail(request.Email))
            {
                result.AddError("Email", "Invalid email format", "INVALID_FORMAT");
            }
            else
            {
                var isEmailUnique = await IsEmailUnique(request.Email, request.BaseAssetHolderId);
                if (!isEmailUnique)
                {
                    result.AddError("Email", "Email address is already in use", "DUPLICATE_EMAIL");
                }
            }
        }
        
        // Validate CPF format and uniqueness
        if (!string.IsNullOrWhiteSpace(request.Cpf))
        {
            if (!IsValidCpf(request.Cpf))
            {
                result.AddError("Cpf", "Invalid CPF format", "INVALID_CPF");
            }
            else
            {
                var isCpfUnique = await IsCpfUnique(request.Cpf, request.BaseAssetHolderId);
                if (!isCpfUnique)
                {
                    result.AddError("Cpf", "CPF is already in use", "DUPLICATE_CPF");
                }
            }
        }
        
        // Validate CNPJ format and uniqueness
        if (!string.IsNullOrWhiteSpace(request.Cnpj))
        {
            if (!IsValidCnpj(request.Cnpj))
            {
                result.AddError("Cnpj", "Invalid CNPJ format", "INVALID_CNPJ");
            }
            else
            {
                var isCnpjUnique = await IsCnpjUnique(request.Cnpj, request.BaseAssetHolderId);
                if (!isCnpjUnique)
                {
                    result.AddError("Cnpj", "CNPJ is already in use", "DUPLICATE_CNPJ");
                }
            }
        }
        
        // Validate that either CPF or CNPJ is provided for Brazilian entities
        // if (string.IsNullOrWhiteSpace(request.Cpf) && string.IsNullOrWhiteSpace(request.Cnpj))
        // {
        //     result.AddError("Cpf", "Either CPF or CNPJ must be provided", "MISSING_IDENTIFICATION");
        //     result.AddError("Cnpj", "Either CPF or CNPJ must be provided", "MISSING_IDENTIFICATION");
        // }
        
        // Validate that both CPF and CNPJ are not provided simultaneously
        if (!string.IsNullOrWhiteSpace(request.Cpf) && !string.IsNullOrWhiteSpace(request.Cnpj))
        {
            result.AddError("Cpf", "Cannot provide both CPF and CNPJ", "CONFLICTING_IDENTIFICATION");
            result.AddError("Cnpj", "Cannot provide both CPF and CNPJ", "CONFLICTING_IDENTIFICATION");
        }
        
        return result;
    }

    public async Task<AssetHolderType> DetermineAssetHolderType(Guid assetHolderId)
    {
        var assetHolder = await _context.BaseAssetHolders
            .Include(bah => bah.Client)
            .Include(bah => bah.Bank)
            .Include(bah => bah.Member)
            .Include(bah => bah.PokerManager)
            .FirstOrDefaultAsync(bah => bah.Id == assetHolderId && !bah.DeletedAt.HasValue);
        
        return assetHolder?.AssetHolderType ?? AssetHolderType.Unknown;
    }

    public async Task<bool> HasActiveTransactions(Guid assetHolderId)
    {
        var walletIdentifierIds = await _context.WalletIdentifiers
            .Where(wi => wi.AssetPool.BaseAssetHolderId == assetHolderId && !wi.DeletedAt.HasValue)
            .Select(wi => wi.Id)
            .ToListAsync();
        
        if (!walletIdentifierIds.Any()) return false;
        
        // Check for active fiat transactions
        var hasFiatTransactions = await _context.FiatAssetTransactions
            .AnyAsync(ft => (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) ||
                           walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)) &&
                          !ft.DeletedAt.HasValue);
        
        if (hasFiatTransactions) return true;
        
        // Check for active digital transactions
        var hasDigitalTransactions = await _context.DigitalAssetTransactions
            .AnyAsync(dt => (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) ||
                           walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)) &&
                          !dt.DeletedAt.HasValue);
        
        if (hasDigitalTransactions) return true;
        
        // Check for active settlement transactions
        var hasSettlementTransactions = await _context.SettlementTransactions
            .AnyAsync(st => (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) ||
                           walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)) &&
                          !st.DeletedAt.HasValue);
        
        return hasSettlementTransactions;
    }

    public async Task<decimal> GetTotalBalance(Guid assetHolderId)
    {
        var walletIdentifiers = await _context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == assetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();
        
        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
        decimal totalBalance = 0;
        
        // Calculate balance from fiat transactions
        var fiatTransactions = await _context.FiatAssetTransactions
            .Where(ft => (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) ||
                         walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)) &&
                        !ft.DeletedAt.HasValue)
            .ToListAsync();
        
        foreach (var transaction in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                transaction.SenderWalletIdentifierId == id || transaction.ReceiverWalletIdentifierId == id);
            
            totalBalance += transaction.GetSignedAmountForWalletIdentifier(relevantWalletId);
        }
        
        // Calculate balance from digital transactions
        var digitalTransactions = await _context.DigitalAssetTransactions
            .Where(dt => (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) ||
                         walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)) &&
                        !dt.DeletedAt.HasValue)
            .ToListAsync();
        
        foreach (var transaction in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                transaction.SenderWalletIdentifierId == id || transaction.ReceiverWalletIdentifierId == id);
            
            totalBalance += transaction.GetSignedAmountForWalletIdentifier(relevantWalletId);
        }
        
        // Calculate balance from settlement transactions
        var settlementTransactions = await _context.SettlementTransactions
            .Where(st => (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) ||
                         walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)) &&
                        !st.DeletedAt.HasValue)
            .ToListAsync();
        
        foreach (var transaction in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                transaction.SenderWalletIdentifierId == id || transaction.ReceiverWalletIdentifierId == id);
            
            totalBalance += transaction.GetSignedAmountForWalletIdentifier(relevantWalletId);
        }
        
        return totalBalance;
    }

    public async Task<DomainValidationResult> ValidateClientCreation(ClientRequest request)
    {
        var result = await ValidateAssetHolderCreation(request);
        
        // Client-specific validations
        if (request.Birthday.HasValue)
        {
            if (request.Birthday.Value > DateTime.Now)
            {
                result.AddError("Birthday", "Birthday cannot be in the future", "FUTURE_DATE");
            }
            
            if (request.Birthday.Value < DateTime.Now.AddYears(-150))
            {
                result.AddError("Birthday", "Birthday cannot be more than 150 years ago", "INVALID_AGE");
            }
        }
        
        return result;
    }

    public async Task<DomainValidationResult> ValidateBankCreation(BankRequest request)
    {
        var result = await ValidateAssetHolderCreation(request);
        
        // Bank-specific validations
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            result.AddError("Code", "Bank code is required", "REQUIRED_FIELD");
        }
        else
        {
            if (request.Code.Length < 1 || request.Code.Length > 10)
            {
                result.AddError("Code", "Bank code must be between 1 and 10 characters", "INVALID_LENGTH");
            }
            
            var isCodeUnique = await IsBankCodeUnique(request.Code);
            if (!isCodeUnique)
            {
                result.AddError("Code", "Bank code is already in use", "DUPLICATE_CODE");
            }
        }
        
        return result;
    }

    public async Task<DomainValidationResult> ValidateMemberCreation(MemberRequest request)
    {
        var result = await ValidateAssetHolderCreation(request);
        
        // Member-specific validations
        if (request.Share.HasValue)
        {
            if (request.Share.Value < 0 || request.Share.Value > 1)
            {
                result.AddError("Share", "Share must be between 0 and 1", "INVALID_RANGE");
            }
        }
        
        if (request.Birthday.HasValue)
        {
            if (request.Birthday.Value > DateTime.Now)
            {
                result.AddError("Birthday", "Birthday cannot be in the future", "FUTURE_DATE");
            }
        }
        
        return result;
    }

    public async Task<DomainValidationResult> ValidatePokerManagerCreation(PokerManagerRequest request)
    {
        var result = await ValidateAssetHolderCreation(request);
        
        // PokerManager-specific validations can be added here
        // Currently inherits all base validations
        
        return result;
    }

    public async Task<bool> HasActiveAssetPools(Guid assetHolderId)
    {
        return await _context.AssetPools
            .AnyAsync(aw => aw.BaseAssetHolderId == assetHolderId && !aw.DeletedAt.HasValue);
    }

    public async Task<bool> IsCpfUnique(string cpf, Guid? excludeAssetHolderId = null)
    {
        var query = _context.BaseAssetHolders
            .Where(bah => bah.Cpf == cpf && !bah.DeletedAt.HasValue);
        
        if (excludeAssetHolderId.HasValue)
        {
            query = query.Where(bah => bah.Id != excludeAssetHolderId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> IsCnpjUnique(string cnpj, Guid? excludeAssetHolderId = null)
    {
        var query = _context.BaseAssetHolders
            .Where(bah => bah.Cnpj == cnpj && !bah.DeletedAt.HasValue);
        
        if (excludeAssetHolderId.HasValue)
        {
            query = query.Where(bah => bah.Id != excludeAssetHolderId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> IsEmailUnique(string email, Guid? excludeAssetHolderId = null)
    {
        var query = _context.BaseAssetHolders
            .Where(bah => bah.Email == email && !bah.DeletedAt.HasValue);
        
        if (excludeAssetHolderId.HasValue)
        {
            query = query.Where(bah => bah.Id != excludeAssetHolderId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> IsBankCodeUnique(string code, Guid? excludeBankId = null)
    {
        var query = _context.Banks
            .Where(b => b.Code == code && !b.DeletedAt.HasValue);
        
        if (excludeBankId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBankId.Value);
        }
        
        return !await query.AnyAsync();
    }

    #region Private Helper Methods

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        
        // Remove non-numeric characters
        cpf = Regex.Replace(cpf, @"[^\d]", "");
        
        // CPF must have 11 digits
        if (cpf.Length != 11) return false;
        
        // Check for known invalid CPFs (all same digits)
        if (cpf.All(c => c == cpf[0])) return false;
        
        // Validate CPF algorithm
        return ValidateCpfAlgorithm(cpf);
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return false;
        
        // Remove non-numeric characters
        cnpj = Regex.Replace(cnpj, @"[^\d]", "");
        
        // CNPJ must have 14 digits
        if (cnpj.Length != 14) return false;
        
        // Check for known invalid CNPJs (all same digits)
        if (cnpj.All(c => c == cnpj[0])) return false;
        
        // Validate CNPJ algorithm
        return ValidateCnpjAlgorithm(cnpj);
    }

    private static bool ValidateCpfAlgorithm(string cpf)
    {
        // CPF validation algorithm
        int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        
        string tempCpf = cpf.Substring(0, 9);
        int sum = 0;
        
        for (int i = 0; i < 9; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];
        
        int remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        
        string digit = remainder.ToString();
        tempCpf = tempCpf + digit;
        sum = 0;
        
        for (int i = 0; i < 10; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];
        
        remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        
        digit = digit + remainder.ToString();
        
        return cpf.EndsWith(digit);
    }

    private static bool ValidateCnpjAlgorithm(string cnpj)
    {
        // CNPJ validation algorithm
        int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        
        string tempCnpj = cnpj.Substring(0, 12);
        int sum = 0;
        
        for (int i = 0; i < 12; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];
        
        int remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        
        string digit = remainder.ToString();
        tempCnpj = tempCnpj + digit;
        sum = 0;
        
        for (int i = 0; i < 13; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];
        
        remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        
        digit = digit + remainder.ToString();
        
        return cnpj.EndsWith(digit);
    }

    #endregion
} 