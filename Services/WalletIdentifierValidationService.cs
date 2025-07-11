using SFManagement.Enums;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums.WalletsMetadata;

namespace SFManagement.Services;

public class WalletIdentifierValidationService
{
    public ValidationResult ValidateWalletIdentifier(WalletIdentifier wallet)
    {
        var result = new ValidationResult();
                
        // Metadata validation based on wallet type
        if (!wallet.ValidateMetadata())
        {
            result.AddError("Metadata", $"Invalid metadata for {wallet.WalletType}");
        }
        
        // Type-specific validation
        switch (wallet.WalletType)
        {
            case WalletType.BankWallet:
                ValidateBankWallet(wallet, result);
                break;
            case WalletType.PokerWallet:
                ValidatePokerWallet(wallet, result);
                break;
            case WalletType.CryptoWallet:
                ValidateCryptoWallet(wallet, result);
                break;
        }
        
        return result;
    }
    
    private void ValidateBankWallet(WalletIdentifier wallet, ValidationResult result)
    {
        var pixKey = wallet.GetBankMetadata(BankWalletMetadata.PixKey);
        var accountNumber = wallet.GetBankMetadata(BankWalletMetadata.AccountNumber);
        var routingNumber = wallet.GetBankMetadata(BankWalletMetadata.RoutingNumber);
        var accountType = wallet.GetBankMetadata(BankWalletMetadata.AccountType);
        
        if (string.IsNullOrEmpty(pixKey))
            result.AddError("PixKey", "Pix key is required for bank wallets");
            
        if (string.IsNullOrEmpty(accountNumber))
            result.AddError("AccountNumber", "Account number is required for bank wallets");
            
        if (string.IsNullOrEmpty(routingNumber))
            result.AddError("RoutingNumber", "Routing number is required for bank wallets");
            
        if (string.IsNullOrEmpty(accountType))
            result.AddError("AccountType", "Account type is required for bank wallets");
            
        // Validate account type
        if (!IsValidAccountType(accountType))
            result.AddError("AccountType", "Invalid account type. Must be 'Checking' or 'Savings'");
    }
    
    private void ValidatePokerWallet(WalletIdentifier wallet, ValidationResult result)
    {
        var inputForTransactions = wallet.GetPokerMetadata(PokerWalletMetadata.InputForTransactions);

        if (string.IsNullOrEmpty(inputForTransactions))
            result.AddError("InputForTransactions", "Input for transactions is required for poker wallets");
    }
    
    private void ValidateCryptoWallet(WalletIdentifier wallet, ValidationResult result)
    {
        var walletAddress = wallet.GetCryptoMetadata(CryptoWalletMetadata.WalletAddress);
        var walletCategory = wallet.GetCryptoMetadata(CryptoWalletMetadata.WalletCategory);
        
        if (string.IsNullOrEmpty(walletAddress))
            result.AddError("WalletAddress", "Wallet address is required for crypto wallets");
            
        if (string.IsNullOrEmpty(walletCategory))
            result.AddError("WalletCategory", "Wallet category is required for crypto wallets");
            
        // Validate wallet category
        if (!IsValidCryptoWalletCategory(walletCategory))
            result.AddError("WalletCategory", "Invalid crypto wallet category. Must be 'Hot', 'Cold', or 'Exchange'");
    }
    
    private bool IsValidAccountType(string? accountType) => 
        accountType is "Checking" or "Savings";
        
    private bool IsValidEmail(string? email) =>
        !string.IsNullOrEmpty(email) && email.Contains('@') && email.Contains('.');
        
    private bool IsValidCryptoWalletCategory(string? category) =>
        category is "Hot" or "Cold" or "Exchange";
}

public class ValidationResult
{
    public List<ValidationError> Errors { get; } = new();
    public bool IsValid => !Errors.Any();
    
    public void AddError(string field, string message)
    {
        Errors.Add(new ValidationError(field, message));
    }
}

public record ValidationError(string Field, string Message); 