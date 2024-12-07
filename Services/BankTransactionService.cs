using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class BankTransactionService : BaseService<BankTransaction>
    {
        public BankTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public override async Task<List<BankTransaction>> List() => context.BankTransactions.Include(x => x.Bank).Include(x => x.Client).Where(x => !x.DeletedAt.HasValue).OrderByDescending(x => x.CreatedAt).ToList();

        public async Task<BankTransaction> Approve(Guid bankTransactionId, ViewModels.BankTransactionApproveRequest model)
        {
            var bankTransaction = _entity.FirstOrDefault(x => x.Id == bankTransactionId);

            if (bankTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação.");

            if (bankTransaction.ApprovedAt.HasValue)
                throw new AppException("Transação já aprovada.");

            bankTransaction.ApprovedAt = DateTime.Now;
            bankTransaction.TagId = model.TagId;
            bankTransaction.ClientId = model.ClientId;

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
            if (context.BankTransactions.Any(x => x.LinkedToId == fromBankTransaction.Id))
                throw new AppException("Esta transação OFX já foi vinculada a uma transação manual.");

            var toBankTransaction = _entity.FirstOrDefault(x => x.Id == toBankTransactionId);

            if (toBankTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação de início.");
            if (!string.IsNullOrEmpty(toBankTransaction.FitId))
                throw new AppException("Não é uma transação de início válida (Não é uma transação manual.)");
            if (toBankTransaction.LinkedToId.HasValue)
                throw new AppException("Esta transação manual já foi vinculada a uma transação OFX.");

            toBankTransaction.LinkedToId = fromBankTransaction.Id;

            toBankTransaction.ApprovedAt = DateTime.Now;

            context.BankTransactions.Update(toBankTransaction);

            fromBankTransaction.ApprovedAt = DateTime.Now;

            context.BankTransactions.Update(fromBankTransaction);

            await context.SaveChangesAsync();

            return (fromBankTransaction, toBankTransaction);
        }

        public async Task<List<BankTransaction>> ListByClientIdAndBankId(Guid? clientId, Guid? bankId) => await context.BankTransactions.Where(x => !x.DeletedAt.HasValue && x.ClientId == clientId && (bankId == null || x.BankId == bankId) && ((string.IsNullOrEmpty(x.FitId) && !x.LinkedToId.HasValue) || (!string.IsNullOrEmpty(x.FitId) && x.ApprovedAt.HasValue))).ToListAsync();

        public async Task<BankTransaction> Unapprove(Guid bankTransactionId)
        {
            var bankTransaction = _entity.FirstOrDefault(x => x.Id == bankTransactionId);

            if (bankTransaction == null)
                throw new AppException("Não foi encontrado nenhuma transação.");

            if (!bankTransaction.ApprovedAt.HasValue)
                throw new AppException("Transação não está aprovada.");

            bankTransaction.ApprovedAt = null;

            if (!string.IsNullOrEmpty(bankTransaction.FitId))
            {
                bankTransaction.ClientId = null;
                var to = _entity.FirstOrDefault(x => x.LinkedToId == bankTransactionId);

                if (to != null)
                {
                    to.ApprovedAt = null;
                    
                    to.LinkedToId = null;
                    
                    context.BankTransactions.Update(to);
                }

            }
            else if (bankTransaction.LinkedToId.HasValue)
            {
                var to = _entity.FirstOrDefault(x => x.Id == bankTransaction.LinkedToId);

                if (to == null)
                    throw new AppException("Não foi encontrado nenhuma transação que tem link com essa transação.");

                to.ApprovedAt = null;

                to.ClientId = null;
                
                bankTransaction.LinkedToId = null;
                
                context.BankTransactions.Update(to);
            }

            context.BankTransactions.Update(bankTransaction);

            await context.SaveChangesAsync();

            return bankTransaction;
        }
    }
}
