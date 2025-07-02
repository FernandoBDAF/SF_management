using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Interfaces;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class FiatAssetTransactionService : BaseTransactionService<FiatAssetTransaction>
{
    public FiatAssetTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public override async Task<FiatAssetTransaction> Add(FiatAssetTransaction model)
    {
        if (!model.WalletIdentifierId.HasValue || model.AssetWalletId == Guid.Empty)
        {
            var walletIdentifierId = context.WalletIdentifiers
                .Where(x => x.BaseAssetHolderId == model.ClientId && x.AssetType == AssetType.BrazilianReal)
                .Select(x => x.Id).SingleOrDefault();
            
            var assetWalletId = context.AssetWallets
                .Where(x => x.BaseAssetHolderId == model.BankId && x.AssetType == AssetType.BrazilianReal)
                .Select(x => x.Id).SingleOrDefault();

            if ((walletIdentifierId == Guid.Empty && !model.FinancialBehaviorId.HasValue) || assetWalletId == Guid.Empty)
            {
                throw new ArgumentException("To create a transaction is needed an AssetWallet + an Wallet identifiers or an FinancialBehaviourId");
            }

            model.WalletIdentifierId = walletIdentifierId == Guid.Empty ? null : walletIdentifierId;
            model.AssetWalletId = assetWalletId;
        }
        
        var transaction = await base.Add(model);
        
        return transaction;
    }
    
    public async Task<FiatAssetTransaction> SendBrazilianReais(Guid baseAssetHolderId, FiatAssetTransactionRequest transaction)
    {
        var assetHolder = await context.BaseAssetHolders
            .Include(x => x.AssetWallets)
            .FirstOrDefaultAsync(x => x.Id == baseAssetHolderId) ?? throw new Exception($"Asset Holder not found");

        var aw = assetHolder.AssetWallets.FirstOrDefault(x => x.AssetType == AssetType.BrazilianReal) ?? throw new Exception($"Asset Wallet for Brazilian Real does not exist");

        var wi = await context.WalletIdentifiers
            .FirstOrDefaultAsync(x => 
                x.AssetType == AssetType.BrazilianReal && (
                    (x.BaseAssetHolderId == transaction.BaseAssetHolderId)
                )) ?? throw new Exception($"Wallet Identifier for Brazilian Real does not exist");


        var fiatTransaction = new FiatAssetTransaction
        {
            AssetWalletId = aw.Id,
            WalletIdentifierId = wi.Id,
            Date = transaction.Date ?? DateTime.Now,
            Description = transaction.Description,
            AssetAmount = transaction.AssetAmount ?? 0,
            TransactionDirection = TransactionDirection.Expense
        };

        await context.FiatAssetTransactions.AddAsync(fiatTransaction);
        await context.SaveChangesAsync();
        
        return fiatTransaction;
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