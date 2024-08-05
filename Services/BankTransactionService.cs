using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;
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
                throw new AppException("Não foi encontrado nenhuma transação.");

            if (bankTransaction.ApprovedAt.HasValue)
                throw new AppException("Transação já aprovada.");

            bankTransaction.ApprovedAt = DateTime.Now;

            context.BankTransactions.Update(bankTransaction);

            await context.SaveChangesAsync();

            return bankTransaction;
        }

        public async Task<(BankTransaction from, BankTransaction to)> Link(Guid fromBankTransactionId, Guid toBankTransactionId)
        {
            var fromBankTransaction = _entity.FirstOrDefault(x => x.Id == fromBankTransactionId);

            if (fromBankTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação de destino.");
            if (string.IsNullOrEmpty(fromBankTransaction.FitId))
                throw new AppException("Não é uma transação de destino válida (Não é uma transação oriunda de arquivo OFX.)");
            if (await context.BankTransactions.AnyAsync(x => x.LinkedToId == fromBankTransaction.Id))
                throw new AppException("Esta transação OFX já foi vinculada a uma transação manual.");

            var toBankTransaction = _entity.FirstOrDefault(x => x.Id == toBankTransactionId);

            if (toBankTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação de início.");
            if (!string.IsNullOrEmpty(toBankTransaction.FitId))
                throw new AppException("Não é uma transação de início válida (Não é uma transação manual.)");
            if (toBankTransaction.LinkedToId.HasValue)
                throw new AppException("Esta transação manual já foi vinculada a uma transação OFX.");

            //TODO: Validações de transações entre clientes diferentes ou entre empresa e cliente.

            toBankTransaction.LinkedToId = fromBankTransaction.Id;

            context.BankTransactions.Update(toBankTransaction);

            await context.SaveChangesAsync();

            return (fromBankTransaction, toBankTransaction);
        }

        public async Task<List<BankTransaction>> ListByClientIdAndBankId(Guid? clientId, Guid? bankId) => await context.BankTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId && (bankId == null || x.BankId == bankId) && ((string.IsNullOrEmpty(x.FitId) && !x.LinkedToId.HasValue) || (!string.IsNullOrEmpty(x.FitId) && x.ApprovedAt.HasValue))).ToListAsync();


    }
}
