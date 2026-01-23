using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Transactions;

public class SettlementTransactionService : BaseTransactionService<SettlementTransaction>
{
    public SettlementTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public async Task<Dictionary<DateTime, List<SettlementTransaction>>> GetClosings(Guid pokerManagerId)
    {
        // Get all wallet identifiers for the poker manager's asset wallets
        var walletIdentifierIds = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == pokerManagerId)
            .Select(wi => wi.Id)
            .ToListAsync();

        var transactions = await context.SettlementTransactions
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(ap => ap.BaseAssetHolder)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(ap => ap.BaseAssetHolder)
            .Where(st => st.DeletedAt == null && 
                        (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) ||
                         walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
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

        // Validate that asset type is compatible with AssetGroup PokerAssets

        // Check if there are any existing settlement transactions for the same date and asset type
        var existingTransactions = await context.SettlementTransactions
            .Include(st => st.SenderWalletIdentifier)
            .Include(st => st.ReceiverWalletIdentifier)
            .AnyAsync(st => st.Date.Date == request.Date.Date && st.DeletedAt == null &&
                            (st.SenderWalletIdentifier.AssetType == request.AssetType ||
                             st.ReceiverWalletIdentifier.AssetType == request.AssetType));

        if (existingTransactions)
        {
            response.Success = false;
            response.Message = $"Settlement transactions already exist for date {request.Date.Date:yyyy-MM-dd} and asset type {request.AssetType}.";
            return response;
        }

        // Validate each transaction
        for (int i = 0; i < request.Transactions.Count; i++)
        {
            var transactionRequest = request.Transactions[i];
            
            // Skip transactions where both assetAmount and rake are zero
            if (transactionRequest.AssetAmount == 0 && transactionRequest.RakeAmount == 0)
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
                        RakeAmount = transactionRequest.RakeAmount,
                        RakeCommission = transactionRequest.RakeCommission,
                        RakeBack = transactionRequest.RakeBack,
                        SenderWalletIdentifierId = transactionRequest.SenderWalletIdentifierId,
                        ReceiverWalletIdentifierId = transactionRequest.ReceiverWalletIdentifierId
                    }
                });
                continue;
            }

            // Validate sender wallet identifier is provided
            if (transactionRequest.SenderWalletIdentifierId == Guid.Empty)
            {
                errors.Add(new SettlementTransactionError
                {
                    Index = i,
                    Error = "SenderWalletIdentifierId is required for settlement transactions",
                    Transaction = new SettlementTransactionRequest
                    {
                        AssetAmount = transactionRequest.AssetAmount,
                        RakeAmount = transactionRequest.RakeAmount,
                        RakeCommission = transactionRequest.RakeCommission,
                        RakeBack = transactionRequest.RakeBack,
                        SenderWalletIdentifierId = transactionRequest.SenderWalletIdentifierId,
                        ReceiverWalletIdentifierId = transactionRequest.ReceiverWalletIdentifierId
                    }
                });
                continue;
            }

            // Validate wallet identifier is required
            if (transactionRequest.ReceiverWalletIdentifierId == Guid.Empty)
            {
                errors.Add(new SettlementTransactionError
                {
                    Index = i,
                    Error = "ReceiverWalletIdentifierId is required for settlement transactions",
                    Transaction = new SettlementTransactionRequest
                    {
                        AssetAmount = transactionRequest.AssetAmount,
                        RakeAmount = transactionRequest.RakeAmount,
                        RakeCommission = transactionRequest.RakeCommission,
                        RakeBack = transactionRequest.RakeBack,
                        SenderWalletIdentifierId = transactionRequest.SenderWalletIdentifierId,
                        ReceiverWalletIdentifierId = transactionRequest.ReceiverWalletIdentifierId
                    }
                });
                continue;
            }

        // Validate that sender and receiver wallet identifiers are different, and have the same asset type
        // and that the asset type is compatible with the asset group PokerAssets and the same of the request

            // If there are validation errors, return them
            if (errors.Count != 0)
            {
                response.Success = false;
                response.Errors = errors;
                response.Message = "Some transactions failed validation.";
                return response;
            }
        }

        // Create all transactions using execution strategy to support retry mechanisms
        var strategy = context.Database.CreateExecutionStrategy();
        
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                using var dbTransaction = await context.Database.BeginTransactionAsync();
                try
                {
                    for (int i = 0; i < request.Transactions.Count; i++)
                    {
                        var transactionRequest = request.Transactions[i];
                        
                        // Skip transactions where both assetAmount and rake are zero
                        if (transactionRequest.AssetAmount == 0 && transactionRequest.RakeAmount == 0)
                        {
                            continue;
                        }
                        
                        var settlementTransaction = new SettlementTransaction
                        {
                            Date = request.Date,
                            SenderWalletIdentifierId = transactionRequest.SenderWalletIdentifierId,
                            ReceiverWalletIdentifierId = transactionRequest.ReceiverWalletIdentifierId,
                            AssetAmount = transactionRequest.AssetAmount,
                            RakeAmount = transactionRequest.RakeAmount,
                            RakeCommission = transactionRequest.RakeCommission,
                            RakeBack = transactionRequest.RakeBack
                        };

                        var createdTransaction = await Add(settlementTransaction);
                        createdTransactions.Add(createdTransaction);
                    }

                    await dbTransaction.CommitAsync();
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            });

            response.Success = true;
            response.CreatedTransactions = [.. createdTransactions.Select(ct => new SettlementTransactionResponse
            {
                Id = ct.Id,
                Date = ct.Date,
                AssetAmount = ct.AssetAmount,
                ApprovedAt = ct.ApprovedAt,
                RakeAmount = ct.RakeAmount,
                RakeCommission = ct.RakeCommission,
                RakeBack = ct.RakeBack,
            })];
            
            response.Message = $"Successfully created {createdTransactions.Count} settlement transactions.";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"Error creating settlement transactions: {ex.Message}";
        }

        return response;
    }
}