using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Examples;

/// <summary>
/// Example usage of InitialBalance functionality
/// </summary>
public class InitialBalanceUsageExample
{
    private readonly DataContext _context;
    private readonly InitialBalanceService _initialBalanceService;

    public InitialBalanceUsageExample(DataContext context, InitialBalanceService initialBalanceService)
    {
        _context = context;
        _initialBalanceService = initialBalanceService;
    }

    /// <summary>
    /// Example of setting initial balances for different asset types
    /// </summary>
    public async Task SetInitialBalancesExample()
    {
        // Assume we have a client with some ID
        var clientId = Guid.NewGuid();
        
        // Set initial balance for BRL (Brazilian Real)
        var brlBalance = await _initialBalanceService.SetInitialBalance(
            clientId, 
            AssetType.BrazilianReal, 
            10000.00m, 
            description: "Initial BRL balance for client"
        );
        
        // Set initial balance for USD with conversion rate
        var usdBalance = await _initialBalanceService.SetInitialBalance(
            clientId,
            AssetType.USDollar,
            2000.00m,
            balanceAs: AssetType.BrazilianReal,
            conversionRate: 5.50m, // 1 USD = 5.50 BRL
            description: "Initial USD balance converted to BRL"
        );
        
        // Set initial balance for Bitcoin
        var btcBalance = await _initialBalanceService.SetInitialBalance(
            clientId,
            AssetType.Bitcoin,
            0.5m,
            description: "Initial Bitcoin balance"
        );
        
        Console.WriteLine($"Set initial balances:");
        Console.WriteLine($"- BRL: {brlBalance.Balance:C}");
        Console.WriteLine($"- USD: {usdBalance.Balance:C} (Rate: {usdBalance.ConversionRate})");
        Console.WriteLine($"- BTC: {btcBalance.Balance} BTC");
    }

    /// <summary>
    /// Example of setting initial balances for asset groups
    /// </summary>
    public async Task SetAssetGroupBalancesExample()
    {
        var clientId = Guid.NewGuid();
        
        // Set initial balance for all fiat assets
        var fiatBalance = await _initialBalanceService.SetInitialBalanceForAssetGroup(
            clientId,
            AssetGroup.FiatAssets,
            50000.00m,
            description: "Total fiat assets balance"
        );
        
        // Set initial balance for all poker assets with conversion
        var pokerBalance = await _initialBalanceService.SetInitialBalanceForAssetGroup(
            clientId,
            AssetGroup.PokerAssets,
            15000.00m,
            balanceAs: AssetType.BrazilianReal,
            conversionRate: 5.35m,
            description: "Total poker assets balance converted to BRL"
        );
        
        // Set initial balance for all crypto assets
        var cryptoBalance = await _initialBalanceService.SetInitialBalanceForAssetGroup(
            clientId,
            AssetGroup.CryptoAssets,
            25000.00m,
            description: "Total crypto assets balance"
        );
        
        Console.WriteLine($"Set asset group balances:");
        Console.WriteLine($"- Fiat Assets: {fiatBalance.Balance:C}");
        Console.WriteLine($"- Poker Assets: {pokerBalance.Balance:C} (converted to {pokerBalance.BalanceAs} at rate {pokerBalance.ConversionRate})");
        Console.WriteLine($"- Crypto Assets: {cryptoBalance.Balance:C}");
    }

    /// <summary>
    /// Example of retrieving and working with initial balances
    /// </summary>
    public async Task RetrieveBalancesExample()
    {
        var clientId = Guid.NewGuid();
        
        // Get all initial balances for a client
        var allBalances = await _initialBalanceService.GetInitialBalancesForAssetHolder(clientId);
        
        Console.WriteLine($"Client has {allBalances.Count} initial balances:");
        foreach (var balance in allBalances)
        {
            if (balance.AssetType != 0)
            {
                Console.WriteLine($"- {balance.AssetType}: {balance.Balance:C}");
            }
            else
            {
                Console.WriteLine($"- {balance.AssetGroup}: {balance.Balance:C}");
            }
        }
        
        // Get specific balance by asset type
        var brlBalance = await _initialBalanceService.GetActiveInitialBalance(clientId, AssetType.BrazilianReal);
        if (brlBalance != null)
        {
            Console.WriteLine($"BRL Balance: {brlBalance.EffectiveBalance:C}");
        }
        
        // Get effective balance for a specific asset type
        var effectiveBrlBalance = await _initialBalanceService.GetEffectiveBalanceForAssetType(clientId, AssetType.BrazilianReal);
        if (effectiveBrlBalance.HasValue)
        {
            Console.WriteLine($"Effective BRL Balance: {effectiveBrlBalance.Value:C}");
        }
        
        // Get balance summary
        var summary = await _initialBalanceService.GetInitialBalanceSummary(clientId);
        Console.WriteLine($"Summary for {summary.BaseAssetHolderName}:");
        Console.WriteLine($"- Asset Type Balances: {summary.TotalAssetTypeBalances}");
        Console.WriteLine($"- Asset Group Balances: {summary.TotalAssetGroupBalances}");
        Console.WriteLine($"- Last Updated: {summary.LastUpdated}");
    }

    /// <summary>
    /// Example of validation and error handling
    /// </summary>
    public async Task ValidationExample()
    {
        var clientId = Guid.NewGuid();
        
        // Validate initial balance data for AssetType
        var validationErrors = await _initialBalanceService.ValidateInitialBalance(
            clientId,
            assetType: AssetType.BrazilianReal,
            balance: 1000.00m
        );
        
        if (validationErrors.Any())
        {
            Console.WriteLine("Validation errors for AssetType:");
            foreach (var error in validationErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        else
        {
            Console.WriteLine("AssetType validation passed!");
        }
        
        // Validate initial balance data for AssetGroup
        var groupValidationErrors = await _initialBalanceService.ValidateInitialBalance(
            clientId,
            assetGroup: AssetGroup.FiatAssets,
            balance: 5000.00m
        );
        
        if (groupValidationErrors.Any())
        {
            Console.WriteLine("Validation errors for AssetGroup:");
            foreach (var error in groupValidationErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        else
        {
            Console.WriteLine("AssetGroup validation passed!");
        }
        
        // Example of trying to set both AssetType and AssetGroup (should fail)
        try
        {
            var invalidErrors = await _initialBalanceService.ValidateInitialBalance(
                clientId,
                assetType: AssetType.BrazilianReal,
                assetGroup: AssetGroup.FiatAssets,
                balance: 1000.00m
            );
            
            if (invalidErrors.Any())
            {
                Console.WriteLine("Expected validation errors (setting both AssetType and AssetGroup):");
                foreach (var error in invalidErrors)
                {
                    Console.WriteLine($"- {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation error: {ex.Message}");
        }
        
        // Example of trying to use conversion rate without BalanceAs (should fail)
        var conversionErrors = await _initialBalanceService.ValidateInitialBalance(
            clientId,
            assetGroup: AssetGroup.FiatAssets,
            balance: 1000.00m,
            conversionRate: 5.35m
        );
        
        if (conversionErrors.Any())
        {
            Console.WriteLine("Expected validation errors (ConversionRate without BalanceAs):");
            foreach (var error in conversionErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
    }

    /// <summary>
    /// Example of updating and removing initial balances
    /// </summary>
    public async Task UpdateAndRemoveExample()
    {
        var clientId = Guid.NewGuid();
        
        // Set initial balance
        var initialBalance = await _initialBalanceService.SetInitialBalance(
            clientId,
            AssetType.BrazilianReal,
            5000.00m,
            description: "Initial BRL balance"
        );
        
        Console.WriteLine($"Set initial balance: {initialBalance.Balance:C}");
        
        // Update the balance (this will soft-delete the old one and create a new one)
        var updatedBalance = await _initialBalanceService.SetInitialBalance(
            clientId,
            AssetType.BrazilianReal,
            7500.00m,
            description: "Updated BRL balance"
        );
        
        Console.WriteLine($"Updated balance: {updatedBalance.Balance:C}");
        
        // Check balance history
        var history = await _initialBalanceService.GetInitialBalanceHistoryByAssetType(clientId, AssetType.BrazilianReal);
        Console.WriteLine($"Balance history has {history.Count} entries");
        
        // Remove the balance
        var removed = await _initialBalanceService.RemoveInitialBalance(clientId, AssetType.BrazilianReal);
        Console.WriteLine($"Balance removed: {removed}");
        
        // Check if client has any balances left
        var hasBalances = await _initialBalanceService.HasInitialBalances(clientId);
        Console.WriteLine($"Client has balances: {hasBalances}");
    }

    /// <summary>
    /// Example of using the unified method for both AssetType and AssetGroup
    /// </summary>
    public async Task UnifiedMethodExample()
    {
        var clientId = Guid.NewGuid();
        
        // Set initial balance for a specific AssetType using unified method
        var brlBalance = await _initialBalanceService.SetInitialBalanceUnified(
            clientId,
            assetType: AssetType.BrazilianReal,
            balance: 10000.00m,
            description: "Initial BRL balance using unified method"
        );
        
        Console.WriteLine($"Set AssetType balance: {brlBalance.AssetType} = {brlBalance.Balance:C}");
        
        // Set initial balance for an AssetGroup using unified method
        var fiatGroupBalance = await _initialBalanceService.SetInitialBalanceUnified(
            clientId,
            assetGroup: AssetGroup.FiatAssets,
            balance: 50000.00m,
            description: "Total fiat assets balance using unified method"
        );
        
        Console.WriteLine($"Set AssetGroup balance: {fiatGroupBalance.AssetGroup} = {fiatGroupBalance.Balance:C}");
        
        // Demonstrate that AssetType is 0 when AssetGroup is set
        Console.WriteLine($"AssetGroup balance - AssetType is: {fiatGroupBalance.AssetType} (should be 0/None)");
        Console.WriteLine($"AssetGroup balance - AssetGroup is: {fiatGroupBalance.AssetGroup} (should be FiatAssets)");
        
        // Demonstrate that AssetGroup is 0 when AssetType is set
        Console.WriteLine($"AssetType balance - AssetType is: {brlBalance.AssetType} (should be BrazilianReal)");
        Console.WriteLine($"AssetType balance - AssetGroup is: {brlBalance.AssetGroup} (should be 0/None)");
    }
} 