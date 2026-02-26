using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Common;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Domain.Interfaces;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Transactions;

public class FiatAssetTransactionService : BaseTransactionService<FiatAssetTransaction>
{
    public FiatAssetTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public override async Task<FiatAssetTransaction> Add(FiatAssetTransaction model)
    {
        // Note: The old properties (WalletIdentifierId, AssetPoolId, ClientId, BankId) 
        // need to be replaced with SenderWalletIdentifierId and ReceiverWalletIdentifierId
        // This method needs to be updated based on your specific business logic
        // for determining sender and receiver wallet identifiers
        
        if (model.SenderWalletIdentifierId == Guid.Empty || model.ReceiverWalletIdentifierId == Guid.Empty)
        {
            throw new ArgumentException("Both SenderWalletIdentifierId and ReceiverWalletIdentifierId are required for transactions");
        }
        
        var transaction = await base.Add(model);
        
        return transaction;
    }
    
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid baseAssetHolderId, FiatAssetTransactionRequest transaction)
    {
        var assetHolder = await context.BaseAssetHolders
            .Include(x => x.AssetPools)
                .ThenInclude(aw => aw.WalletIdentifiers)
            .FirstOrDefaultAsync(x => x.Id == baseAssetHolderId) ?? throw new Exception($"Asset Holder not found");

        var senderWallet = assetHolder.AssetPools.FirstOrDefault(x => x.AssetGroup == AssetGroup.FiatAssets) 
            ?? throw new Exception($"Asset Wallet for Fiat Assets does not exist");

        var senderIdentifier = senderWallet.WalletIdentifiers.FirstOrDefault(wi => wi.AssetType == AssetType.BrazilianReal)
            ?? throw new Exception($"No wallet identifier found for Brazilian Real");

        // Find receiver wallet identifier based on the transaction request
        var receiverIdentifier = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .FirstOrDefaultAsync(x => 
                x.AssetType == AssetType.BrazilianReal && 
                x.AssetPool!.BaseAssetHolderId == transaction.BaseAssetHolderId) 
            ?? throw new Exception($"Receiver Wallet Identifier for Brazilian Real does not exist");

        var fiatTransaction = new FiatAssetTransaction
        {
            SenderWalletIdentifierId = senderIdentifier.Id,
            ReceiverWalletIdentifierId = receiverIdentifier.Id,
            Date = transaction.Date,
            CategoryId = transaction.CategoryId,
            AssetAmount = transaction.AssetAmount
        };

        await context.FiatAssetTransactions.AddAsync(fiatTransaction);
        await context.SaveChangesAsync();
        
        return fiatTransaction;
    }

    public async Task<FiatAssetTransaction> PartialUpdateAsync(Guid id, UpdateFiatAssetTransactionRequest request)
    {
        var entity = await context.FiatAssetTransactions
            .FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);

        if (entity == null)
        {
            throw new KeyNotFoundException($"FiatAssetTransaction with ID {id} not found.");
        }

        if (entity.ApprovedAt.HasValue)
        {
            throw new InvalidOperationException("Cannot update an approved transaction. Remove approval first.");
        }

        if (request.Date.HasValue)
        {
            entity.Date = request.Date.Value;
        }

        if (request.AssetAmount.HasValue)
        {
            entity.AssetAmount = request.AssetAmount.Value;
        }

        if (request.CategoryId.HasValue)
        {
            entity.CategoryId = request.CategoryId.Value == Guid.Empty
                ? null
                : request.CategoryId;
        }

        await context.SaveChangesAsync();
        return entity;
    }

    // public override async Task<List<FiatAssetTransaction>> List()
    // {
    //     await Task.Yield();
    //     // return await context.BankTransactions.Include(x => x.Bank).Include(x => x.Client)
    //     //     .Where(x => !x.DeletedAt.HasValue).OrderByDescending(x => x.CreatedAt).ToListAsync();
    //     return null;
    // }

    // public async Task<FiatAssetTransaction> Approve(Guid bankTransactionId, BankTransactionApproveRequest model)
    // {
    //     var bankTransaction = _entity.FirstOrDefault(x => x.Id == bankTransactionId);

    //     if (bankTransaction == null) throw new AppException("Não foi encontrado nenhuma transação.");

    //     if (bankTransaction.ApprovedAt.HasValue) throw new AppException("Transação já aprovada.");

    //     bankTransaction.ApprovedAt = DateTime.Now;
    //     // bankTransaction.TagId = model.TagId;
    //     // bankTransaction.ClientId = model.ClientId;
    //     // bankTransaction.ManagerId = model.ManagerId;

    //     if (_user != null)
    //         bankTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);

    //     // context.BankTransactions.Update(bankTransaction);

    //     await context.SaveChangesAsync();

    //     return bankTransaction;
    // }

    // public async Task<(FiatAssetTransaction from, FiatAssetTransaction to)> Link(Guid fromBankTransactionId,
    //     Guid toBankTransactionId)
    // {
    //     // var fromBankTransaction = _entity.FirstOrDefault(x => x.Id == fromBankTransactionId);
    //     //
    //     // if (fromBankTransaction == null) throw new AppException("Não foi encontrado nenhuma transação de destino.");
    //     // if (string.IsNullOrEmpty(fromBankTransaction.FitId))
    //     //     throw new AppException(
    //     //         "Não é uma transação de destino válida (Não é uma transação oriunda de arquivo OFX.)");
    //     // if (context.BankTransactions.Any(x => x.LinkedToId == fromBankTransaction.Id))
    //     //     throw new AppException("Esta transação OFX já foi vinculada a uma transação manual.");
    //     //
    //     // var toBankTransaction = _entity.FirstOrDefault(x => x.Id == toBankTransactionId);
    //     //
    //     // if (toBankTransaction == null) throw new AppException("Não foi encontrado nenhuma transação de início.");
    //     // if (!string.IsNullOrEmpty(toBankTransaction.FitId))
    //     //     throw new AppException("Não é uma transação de início válida (Não é uma transação manual.)");
    //     // if (toBankTransaction.LinkedToId.HasValue)
    //     //     throw new AppException("Esta transação manual já foi vinculada a uma transação OFX.");
    //     //
    //     // toBankTransaction.LinkedToId = fromBankTransaction.Id;
    //     // toBankTransaction.ApprovedAt = DateTime.Now;
    //     //
    //     // if (_user != null)
    //     //     toBankTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
    //     //
    //     // context.BankTransactions.Update(toBankTransaction);
    //     //
    //     // fromBankTransaction.ApprovedAt = DateTime.Now;
    //     //
    //     // if (_user != null)
    //     //     fromBankTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
    //     //
    //     // context.BankTransactions.Update(fromBankTransaction);
    //     //
    //     // await context.SaveChangesAsync();
    //     //
    //     // return (fromBankTransaction, toBankTransaction);
    //     await Task.Yield();
    //     return (null, null);
    // }

    // public async Task<List<FiatAssetTransaction>> ListByClientIdAndBankId(Guid? clientId, Guid? bankId)
    // {
    //     // return await context.BankTransactions.Where(x =>
    //     //     !x.DeletedAt.HasValue && x.ClientId == clientId && (bankId == null || x.BankId == bankId) &&
    //     //     ((string.IsNullOrEmpty(x.FitId) && !x.LinkedToId.HasValue) ||
    //     //      (!string.IsNullOrEmpty(x.FitId) && x.ApprovedAt.HasValue))).ToListAsync();
    //     await Task.Yield();
    //     return null;
    // }

    // public async Task<FiatAssetTransaction> Unapprove(Guid bankTransactionId)
    // {
    //     // var bankTransaction = _entity.FirstOrDefault(x => x.Id == bankTransactionId);
    //     //
    //     // if (bankTransaction == null) throw new AppException("Não foi encontrado nenhuma transação.");
    //     // if (!bankTransaction.ApprovedAt.HasValue) throw new AppException("Transação não está aprovada.");
    //     //
    //     // bankTransaction.ApprovedAt = null;
    //     // bankTransaction.ApprovedBy = null;
    //     // bankTransaction.TagId = null;
    //     //
    //     // if (!string.IsNullOrEmpty(bankTransaction.FitId))
    //     // {
    //     //     bankTransaction.ClientId = null;
    //     //     var to = _entity.FirstOrDefault(x => x.LinkedToId == bankTransactionId);
    //     //
    //     //     if (to != null)
    //     //     {
    //     //         to.ApprovedAt = null;
    //     //         to.ApprovedBy = null;
    //     //         to.LinkedToId = null;
    //     //
    //     //         to.TagId = null;
    //     //
    //     //         context.BankTransactions.Update(to);
    //     //     }
    //     // }
    //     // else if (bankTransaction.LinkedToId.HasValue)
    //     // {
    //     //     var to = _entity.FirstOrDefault(x => x.Id == bankTransaction.LinkedToId);
    //     //
    //     //     if (to == null)
    //     //         throw new AppException("Não foi encontrado nenhuma transação que tem link com essa transação.");
    //     //
    //     //     to.ApprovedAt = null;
    //     //     to.ApprovedBy = null;
    //     //     to.ClientId = null;
    //     //
    //     //     to.TagId = null;
    //     //
    //     //     bankTransaction.LinkedToId = null;
    //     //
    //     //     context.BankTransactions.Update(to);
    //     // }
    //     //
    //     // context.BankTransactions.Update(bankTransaction);
    //     //
    //     // await context.SaveChangesAsync();
    //     //
    //     // return bankTransaction;
    //     await Task.Yield();
    //     return null;
    // }
}