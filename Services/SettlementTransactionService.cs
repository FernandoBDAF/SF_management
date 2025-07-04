using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Transactions;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class SettlementTransactionService : BaseTransactionService<SettlementTransaction>
{
    public SettlementTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public async Task<Dictionary<DateTime, List<SettlementTransaction>>> GetClosings(Guid pokerManagerId)
    {
        var transactions = await context.SettlementTransactions
            .Include(st => st.AssetWallet)
            .Include(st => st.WalletIdentifier)
            .Where(st => st.DeletedAt == null && 
                        st.AssetWallet.BaseAssetHolderId == pokerManagerId)
            .OrderBy(st => st.Date)
            .ToListAsync();

        var groupedTransactions = transactions
            .GroupBy(st => st.Date.Date) // Group by date only (without time)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

        return groupedTransactions;
    }

    public async Task<SettlementTransactionByDateResponse> CreateSettlementTransactionsByDate(
        Guid assetHolderId, 
        SettlementTransactionByDateRequest request)
    {
        var response = new SettlementTransactionByDateResponse();
        var errors = new List<SettlementTransactionError>();
        var createdTransactions = new List<SettlementTransaction>();

        // Validate AssetWalletId is provided
        if (!request.AssetWalletId.HasValue)
        {
            response.Success = false;
            response.Message = "AssetWalletId is required in the request.";
            return response;
        }

        // Validate asset wallet belongs to the specified asset holder
        var assetWallet = await context.AssetWallets
            .FirstOrDefaultAsync(aw => aw.Id == request.AssetWalletId.Value &&
                                     aw.BaseAssetHolderId == assetHolderId &&
                                     !aw.DeletedAt.HasValue);

        if (assetWallet == null)
        {
            response.Success = false;
            response.Message = $"Asset wallet {request.AssetWalletId.Value} not found or does not belong to asset holder {assetHolderId}.";
            return response;
        }

        // Check if there are any existing settlement transactions for the same date
        var existingTransactions = await context.SettlementTransactions
            .AnyAsync(st => st.Date.Date == request.Date.Date &&
                           st.AssetWalletId == request.AssetWalletId.Value &&
                           st.DeletedAt == null);

        if (existingTransactions)
        {
            response.Success = false;
            response.Message = $"Settlement transactions already exist for date {request.Date.Date:yyyy-MM-dd} and asset wallet {request.AssetWalletId.Value}.";
            return response;
        }

        // Validate each transaction
        for (int i = 0; i < request.Transactions.Count; i++)
        {
            var transactionRequest = request.Transactions[i];
            
            // Skip transactions where both assetAmount and rake are zero
            if (transactionRequest.AssetAmount == 0 && transactionRequest.Rake == 0)
            {
                continue;
            }
            
            // Validate required fields
            if (transactionRequest.AssetAmount < 0)
            {
                errors.Add(new SettlementTransactionError
                {
                    Index = i,
                    Error = "AssetAmount must be equal or greater than 0",
                    Transaction = new SettlementTransactionRequest
                    {
                        AssetAmount = transactionRequest.AssetAmount,
                        TransactionDirection = transactionRequest.TransactionDirection,
                        Rake = transactionRequest.Rake,
                        RakeCommission = transactionRequest.RakeCommission,
                        RakeBack = transactionRequest.RakeBack,
                        WalletIdentifierId = transactionRequest.WalletIdentifierId
                    }
                });
                continue;
            }

            // Validate wallet identifier is required
            if (!transactionRequest.WalletIdentifierId.HasValue)
            {
                errors.Add(new SettlementTransactionError
                {
                    Index = i,
                    Error = "WalletIdentifierId is required for settlement transactions",
                    Transaction = new SettlementTransactionRequest
                    {
                        AssetAmount = transactionRequest.AssetAmount,
                        TransactionDirection = transactionRequest.TransactionDirection,
                        Rake = transactionRequest.Rake,
                        RakeCommission = transactionRequest.RakeCommission,
                        RakeBack = transactionRequest.RakeBack,
                        WalletIdentifierId = transactionRequest.WalletIdentifierId
                    }
                });
                continue;
            }

            // Validate wallet identifier exists
            var walletIdentifier = await context.WalletIdentifiers
                .FirstOrDefaultAsync(wi => wi.Id == transactionRequest.WalletIdentifierId.Value &&
                                         !wi.DeletedAt.HasValue);
            
            if (walletIdentifier == null)
            {
                errors.Add(new SettlementTransactionError
                {
                    Index = i,
                    Error = $"Wallet identifier {transactionRequest.WalletIdentifierId.Value} not found",
                    Transaction = new SettlementTransactionRequest
                    {
                        AssetAmount = transactionRequest.AssetAmount,
                        TransactionDirection = transactionRequest.TransactionDirection,
                        Rake = transactionRequest.Rake,
                        RakeCommission = transactionRequest.RakeCommission,
                        RakeBack = transactionRequest.RakeBack,
                        WalletIdentifierId = transactionRequest.WalletIdentifierId
                    }
                });
                continue;
            }

            // Validate that wallet identifier has the same asset type as the asset wallet
            if (walletIdentifier.AssetType != assetWallet.AssetType)
            {
                errors.Add(new SettlementTransactionError
                {
                    Index = i,
                    Error = $"Wallet identifier {transactionRequest.WalletIdentifierId.Value} has asset type {walletIdentifier.AssetType} but asset wallet has asset type {assetWallet.AssetType}",
                    Transaction = new SettlementTransactionRequest
                    {
                        AssetAmount = transactionRequest.AssetAmount,
                        TransactionDirection = transactionRequest.TransactionDirection,
                        Rake = transactionRequest.Rake,
                        RakeCommission = transactionRequest.RakeCommission,
                        RakeBack = transactionRequest.RakeBack,
                        WalletIdentifierId = transactionRequest.WalletIdentifierId
                    }
                });
                continue;
            }
        }

        // If there are validation errors, return them
        if (errors.Count != 0)
        {
            response.Success = false;
            response.Errors = errors;
            response.Message = "Some transactions failed validation.";
            return response;
        }

        // Create all transactions in a transaction
        using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            for (int i = 0; i < request.Transactions.Count; i++)
            {
                var transactionRequest = request.Transactions[i];
                
                // Skip transactions where both assetAmount and rake are zero
                if (transactionRequest.AssetAmount == 0 && transactionRequest.Rake == 0)
                {
                    continue;
                }
                
                var settlementTransaction = new SettlementTransaction
                {
                    Date = request.Date,
                    AssetWalletId = request.AssetWalletId.Value,
                    AssetAmount = transactionRequest.AssetAmount,
                    TransactionDirection = transactionRequest.TransactionDirection,
                    Description = null, // Not provided in reduced request
                    WalletIdentifierId = transactionRequest.WalletIdentifierId,
                    FinancialBehaviorId = null, // Not provided in reduced request
                    Rake = transactionRequest.Rake,
                    RakeCommission = transactionRequest.RakeCommission,
                    RakeBack = transactionRequest.RakeBack
                };

                // Use base service Add method to handle audit fields
                var createdTransaction = await Add(settlementTransaction);
                createdTransactions.Add(createdTransaction);
            }

            await dbTransaction.CommitAsync();

            response.Success = true;
            response.CreatedTransactions = [.. createdTransactions.Select(ct => new SettlementTransactionResponse
            {
                Id = ct.Id,
                Date = ct.Date,
                AssetAmount = ct.AssetAmount,
                TransactionDirection = ct.TransactionDirection,
                Description = ct.Description,
                ApprovedAt = ct.ApprovedAt,
                Rake = ct.Rake,
                RakeCommission = ct.RakeCommission,
                RakeBack = ct.RakeBack
            })];
            
            response.Message = $"Successfully created {createdTransactions.Count} settlement transactions.";
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            response.Success = false;
            response.Message = $"Error creating settlement transactions: {ex.Message}";
        }

        return response;
    }
}