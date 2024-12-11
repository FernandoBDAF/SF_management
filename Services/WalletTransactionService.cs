using AutoMapper;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class WalletTransactionService : BaseService<WalletTransaction>
    {
        private readonly IMapper _mapper;

        public WalletTransactionService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
            _mapper = mapper;
        }

        public async Task<WalletTransactionResponse> ApproveTransaction(Guid walletTransactionId, WalletTransactionApproveRequest model)
        {
            var walletTransaction = await base.Get(walletTransactionId);
            if (walletTransaction == null)
            {
                throw new AppException("Wallet transaction not found");
            }

            walletTransaction.ApprovedAt = DateTime.Now;
            walletTransaction.NicknameId = model.NicknameId;
            walletTransaction.TagId = model.TagId;
            walletTransaction.ClientId = model.ClientId;
            walletTransaction.WalletId = model.WalletId;
            walletTransaction.ExchangeRate = model.ExchangeRate;
            walletTransaction.Value = model.Value;

            context.WalletTransactions.Update(walletTransaction);
            await context.SaveChangesAsync();

            return _mapper.Map<WalletTransactionResponse>(walletTransaction);
        }

        public async Task<WalletTransaction> UnApproveTransaction(Guid walletTransactionId)
        {
            var walletTransaction = _entity.FirstOrDefault(x => x.Id == walletTransactionId);

            if (walletTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação.");

            if (!walletTransaction.ApprovedAt.HasValue)
                throw new AppException("Transação não está aprovada.");

            walletTransaction.ApprovedAt = null;
            
            walletTransaction.TagId = null;

            if (walletTransaction.ExcelId.HasValue)
            {
                walletTransaction.ClientId = null;
                walletTransaction.WalletId = null;
                walletTransaction.ExchangeRate = 0;
                walletTransaction.Value = 0;
                walletTransaction.TagId = null;
                var to = _entity.FirstOrDefault(x => x.LinkedToId == walletTransactionId);

                if (to != null)
                {
                    to.ApprovedAt = null;
                    
                    to.LinkedToId = null;
                    
                    context.WalletTransactions.Update(to);
                }

            }
            else if (walletTransaction.LinkedToId.HasValue)
            {
                var to = _entity.FirstOrDefault(x => x.Id == walletTransaction.LinkedToId);

                if (to == null)
                    throw new AppException("Não foi encontrado nenhuma transação que tem link com essa transação.");

                to.ApprovedAt = null;
                to.ClientId = null;
                to.WalletId = null;
                to.ExchangeRate = 0;
                to.Value = 0;
                to.TagId = null;
                
                walletTransaction.LinkedToId = null;
                
                context.WalletTransactions.Update(to);
            }

            context.WalletTransactions.Update(walletTransaction);

            await context.SaveChangesAsync();

            return walletTransaction;
        }

        public async Task<(WalletTransaction from, WalletTransaction to)> Link(Guid fromWalletTransactionId, Guid toWalletTransactionId)
        {
            var fromWalletTransaction = _entity.FirstOrDefault(x => x.Id == fromWalletTransactionId);

            if (fromWalletTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação de destino.");
            if (!fromWalletTransaction.ExcelId.HasValue)
                throw new AppException("Não é uma transação de destino válida (Não é uma transação oriunda de arquivo EXCEL.)");
            if (context.WalletTransactions.Any(x => x.LinkedToId == fromWalletTransaction.Id))
                throw new AppException("Esta transação EXCEL já foi vinculada a uma transação manual.");

            var toWalletTransaction = _entity.FirstOrDefault(x => x.Id == toWalletTransactionId);

            if (toWalletTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação de início.");
            if (toWalletTransaction.ExcelId.HasValue)
                throw new AppException("Não é uma transação de início válida (Não é uma transação manual.)");
            if (toWalletTransaction.LinkedToId.HasValue)
                throw new AppException("Esta transação manual já foi vinculada a uma transação EXCEL.");

            toWalletTransaction.LinkedToId = fromWalletTransaction.Id;

            toWalletTransaction.ApprovedAt = DateTime.Now;

            context.WalletTransactions.Update(toWalletTransaction);

            fromWalletTransaction.ApprovedAt = DateTime.Now;

            context.WalletTransactions.Update(fromWalletTransaction);

            await context.SaveChangesAsync();

            return (fromWalletTransaction, toWalletTransaction);
        }
    }
}
