using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class WalletTransactionService : BaseService<WalletTransaction>
    {
        private readonly IMapper _mapper;
        private readonly ClaimsPrincipal _user;

        public WalletTransactionService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
            _mapper = mapper;
            _user = httpContextAccessor.HttpContext?.User;
        }

        public override async Task<WalletTransaction> Add(WalletTransaction obj)
        {
            obj = await base.Add(obj);
            obj = await ExecuteFinanceCalc(obj);

            return obj;
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

            if (_user != null)
            {
                walletTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
            }

            context.WalletTransactions.Update(walletTransaction);
            await context.SaveChangesAsync();

            walletTransaction = await ExecuteFinanceCalc(walletTransaction);

            return _mapper.Map<WalletTransactionResponse>(walletTransaction);
        }

        public async Task<WalletTransaction> UnApproveTransaction(Guid walletTransactionId)
        {
            var walletTransaction = _entity.FirstOrDefault(x => x.Id == walletTransactionId);

            if (walletTransaction == null)
            {
                throw new AppException("Não foi encontrado nenhuma transação.");
            }

            if (!walletTransaction.ApprovedAt.HasValue)
            {
                throw new AppException("Transação não está aprovada.");
            }

            walletTransaction.ApprovedAt = null;
            walletTransaction.ApprovedBy = null;
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
                    to.ApprovedBy = null;
                    to.LinkedToId = null;

                    context.WalletTransactions.Update(to);
                }

            }
            else if (walletTransaction.LinkedToId.HasValue)
            {
                var to = _entity.FirstOrDefault(x => x.Id == walletTransaction.LinkedToId);

                if (to == null)
                {
                    throw new AppException("Não foi encontrado nenhuma transação que tem link com essa transação.");
                }

                to.ApprovedAt = null;
                to.ApprovedBy = null;
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
            {
                throw new AppException("Não foi encontrado nenhuma transação de destino.");
            }
            if (!fromWalletTransaction.ExcelId.HasValue)
            {
                throw new AppException("Não é uma transação de destino válida (Não é uma transação oriunda de arquivo EXCEL.)");
            }
            if (context.WalletTransactions.Any(x => x.LinkedToId == fromWalletTransaction.Id))
            {
                throw new AppException("Esta transação EXCEL já foi vinculada a uma transação manual.");
            }

            var toWalletTransaction = _entity.FirstOrDefault(x => x.Id == toWalletTransactionId);

            if (toWalletTransaction == null)
            {
                throw new AppException("Não foi encontrado nenhuma transação de início.");
            }
            if (toWalletTransaction.ExcelId.HasValue)
            {
                throw new AppException("Não é uma transação de início válida (Não é uma transação manual.)");
            }
            if (toWalletTransaction.LinkedToId.HasValue)
            {
                throw new AppException("Esta transação manual já foi vinculada a uma transação EXCEL.");
            }

            toWalletTransaction.LinkedToId = fromWalletTransaction.Id;

            if (_user != null)
            {
                toWalletTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
            }

            toWalletTransaction.ApprovedAt = DateTime.Now;

            context.WalletTransactions.Update(toWalletTransaction);

            fromWalletTransaction.ApprovedAt = DateTime.Now;

            if (_user != null)
            {
                fromWalletTransaction.ApprovedBy = Guid.Parse(_user.Claims.FirstOrDefault(c => c.Type == "uid").Value);
            }

            context.WalletTransactions.Update(fromWalletTransaction);

            await context.SaveChangesAsync();

            return (fromWalletTransaction, toWalletTransaction);
        }

        public async Task<WalletTransaction> ExecuteFinanceCalc(WalletTransaction obj)
        {
            if (obj.ManagerId.HasValue)
            {
                var manager = await context.Managers.FirstOrDefaultAsync(x => x.Id == obj.ManagerId);

                var queryWalletTransactions = context.WalletTransactions.Where(x => !x.DeletedAt.HasValue && (!x.TagId.HasValue) && ((!x.ApprovedAt.HasValue) || (x.ApprovedAt.HasValue && x.LinkedToId.HasValue)) && x.ManagerId == obj.ManagerId).OrderByDescending(x => !x.DeletedAt.HasValue).ThenBy(x => x.WalletTransactionType);

                obj = await CalcFinance(queryWalletTransactions, manager, obj);

                context.WalletTransactions.Update(obj);

                foreach (var walletTransaction in await queryWalletTransactions.Where(x => x.Date >= obj.Date && x.Id != obj.Id).ToListAsync())
                {
                    context.WalletTransactions.Update(await CalcFinance(queryWalletTransactions, manager, walletTransaction));
                }

                await context.SaveChangesAsync();
            }

            return obj;
        }

        public async Task<WalletTransaction> CalcFinance(IOrderedQueryable<WalletTransaction> queryWalletTransactions, Manager manager, WalletTransaction obj)
        {
            var lastTransaction = await queryWalletTransactions.FirstOrDefaultAsync(x => x.Date < obj.Date && x.Id != obj.Id);

            if (obj.WalletTransactionType == Enums.WalletTransactionType.Income)
            {
                if (lastTransaction == null)
                {
                    obj.AverateRate = (manager.InitialCoins + obj.Coins) / ((manager.InitialCoins * manager.InitialExchangeRate) + (obj.Coins * obj.ExchangeRate));
                }
                else
                {
                    var balanceCoins = await queryWalletTransactions.Where(x => x.Date < obj.Date && x.Id != obj.Id).SumAsync(x => x.Coins);

                    obj.AverateRate = (balanceCoins + obj.Coins) / ((balanceCoins * lastTransaction.ExchangeRate) + (obj.Coins * obj.ExchangeRate));
                }
            }
            else if (obj.WalletTransactionType == Enums.WalletTransactionType.Expense)
            {
                if (lastTransaction == null)
                {
                    obj.AverateRate = manager.InitialExchangeRate;
                }
                else
                {
                    obj.AverateRate = lastTransaction.AverateRate;
                }

                obj.Profit = (obj.ExchangeRate - obj.AverateRate) * obj.Coins;
            }

            return obj;
        }
    }
}
