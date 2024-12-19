using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class InternalTransactionService : BaseService<InternalTransaction>
    {
        public InternalTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public async Task<List<InternalTransaction>> Transfer(Guid toId, Guid fromId, InternalTransactionTransferRequest obj)
        {
            var transferId = Guid.NewGuid();

            var toInternalTransaction = new InternalTransaction()
            {
                Value = obj.Value,
                Coins = obj.Coins,
                ExchangeRate = obj.ExchangeRate,
                ClientId = toId,
                TransferId = transferId,
                InternalTransactionType = Enums.InternalTransactionType.Income,
                Date = obj.Date,
                Description = obj.Description
            };

            var fromInternalTransaction = new InternalTransaction()
            {
                Value = obj.Value,
                Coins = obj.Coins,
                ExchangeRate = obj.ExchangeRate,
                ClientId = fromId,
                TransferId = transferId,
                InternalTransactionType = Enums.InternalTransactionType.Expense,
                Date = obj.Date,
                Description = obj.Description
            };

            await _entity.AddAsync(toInternalTransaction);
            await _entity.AddAsync(fromInternalTransaction);

            await context.SaveChangesAsync();

            return new List<InternalTransaction> { toInternalTransaction, fromInternalTransaction };
        }

        public async Task<InternalTransaction> Approve(Guid internalTransactionId, ViewModels.InternalTransactionApproveRequest model)
        {
            var internalTransaction = _entity.FirstOrDefault(x => x.Id == internalTransactionId);

            if (internalTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação.");

            if (internalTransaction.ApprovedAt.HasValue)
                throw new AppException("Transação já aprovada.");

            internalTransaction.ApprovedAt = DateTime.Now;
            internalTransaction.TagId = model.TagId;
            internalTransaction.ClientId = model.ClientId;
            internalTransaction.ManagerId = model.ManagerId;
            internalTransaction.BankId = model.BankId;


            context.InternalTransactions.Update(internalTransaction);

            await context.SaveChangesAsync();

            return internalTransaction;
        }


        public async Task<InternalTransaction> Unapprove(Guid internalTransactionId)
        {
            var internalTransaction = _entity.FirstOrDefault(x => x.Id == internalTransactionId);

            if (internalTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação.");

            if (!internalTransaction.ApprovedAt.HasValue)
                throw new AppException("Transação não está aprovada.");

            internalTransaction.ApprovedAt = null;

            context.InternalTransactions.Update(internalTransaction);

            await context.SaveChangesAsync();

            return internalTransaction;
        }


        public override async Task Delete(Guid id)
        {
            var obj = await _entity.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);

            if (obj != null)
            {
                obj.DeletedAt = DateTime.Now;

                if (obj.TransferId.HasValue)
                {
                    var anotherTransaction = await _entity.FirstOrDefaultAsync(x => x.TransferId == obj.TransferId && !x.DeletedAt.HasValue);

                    if (anotherTransaction != null)
                    {
                        anotherTransaction.DeletedAt = DateTime.Now;
                        _entity.Update(anotherTransaction);
                    }
                }

                _entity.Update(obj);

                await context.SaveChangesAsync();
            }
        }
    }
}
