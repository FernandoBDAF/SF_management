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
                InternalTransactionType = Enums.InternalTransactionType.Income
            };

            var fromInternalTransaction = new InternalTransaction()
            {
                Value = obj.Value,
                Coins = obj.Coins,
                ExchangeRate = obj.ExchangeRate,
                ClientId = fromId,
                TransferId = transferId,
                InternalTransactionType = Enums.InternalTransactionType.Expense
            };

            await _entity.AddAsync(toInternalTransaction);
            await _entity.AddAsync(fromInternalTransaction);

            await context.SaveChangesAsync();

            return new List<InternalTransaction> { toInternalTransaction, fromInternalTransaction };
        }
    }
}
