using SFManagement.Data;
using SFManagement.Models;
using System.Data.Entity;

namespace SFManagement.Services
{
    public class BankTransactionService : BaseService<BankTransaction>
    {
        public BankTransactionService(DataContext context) : base(context)
        {
        }

        public async Task<BankTransaction> Approve(Guid bankTransactionId)
        {
            var bankTransaction = _entity.FirstOrDefault(x => x.Id == bankTransactionId);

            if (bankTransaction == null)
                throw new Exception("Não foi encontrado nenhuma transação.");

            if(bankTransaction.ApprovedAt.HasValue)
                throw new Exception("Transação já aprovada.");

            bankTransaction.ApprovedAt = DateTime.Now;

            context.BankTransactions.Update(bankTransaction);

            await context.SaveChangesAsync();

            return bankTransaction;
        }
    }
}
