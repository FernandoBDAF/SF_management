using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class TransactionService
    {
        private readonly DataContext _context;

        public TransactionService(DataContext context)
        {
            _context = context;
        }

        public async Task<TableResponse<TransactionResponse>> GetWalletTransactions(Guid walletId, DateTime? startDate, DateTime? endDate, int quantity, int page)
        {
            var response = new TableResponse<TransactionResponse>()
            {
                Page = page,
                Show = quantity
            };

            var walletTransactionsQuery = _context.WalletTransactions.Where(x => x.WalletId == walletId);

            if (startDate.HasValue)
            {
                walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
            }

            response.Total = walletTransactionsQuery.Count();

            var allTransactions = new List<TransactionResponse>();
            allTransactions.AddRange((walletTransactionsQuery.ToList()).Select(x => new TransactionResponse(x)));

            response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();

            return response;
        }

        public async Task<TableResponse<TransactionResponse>> GetBankTransactions(Guid bankId, DateTime? startDate, DateTime? endDate, int quantity, int page)
        {
            var response = new TableResponse<TransactionResponse>()
            {
                Page = page,
                Show = quantity
            };

            var bankTransactionsQuery = _context.BankTransactions.Where(x => x.BankId == bankId);

            if (startDate.HasValue)
            {
                bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
            }

            response.Total = bankTransactionsQuery.Count();

            var allTransactions = new List<TransactionResponse>();
            allTransactions.AddRange((await bankTransactionsQuery.ToListAsync()).Select(x => new TransactionResponse(x)));

            response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();

            return response;
        }

        public async Task<TableResponse<TransactionResponse>> GetManagerTransactions(Guid managerId, DateTime? startDate, DateTime? endDate, int quantity, int page)
        {
            var response = new TableResponse<TransactionResponse>()
            {
                Page = page,
                Show = quantity
            };

            var walletTransactionsQuery = _context.WalletTransactions.Include(x => x.Wallet).Where(x => x.Wallet != null && x.Wallet.ManagerId == managerId);

            if (startDate.HasValue)
            {
                walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
            }

            response.Total = walletTransactionsQuery.Count();

            var allTransactions = new List<TransactionResponse>();
            allTransactions.AddRange((walletTransactionsQuery.ToList()).Select(x => new TransactionResponse(x)));

            response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();

            return response;
        }

        public async Task<TableResponse<TransactionResponse>> GetTransactions(Guid clientId, DateTime? startDate, DateTime? endDate, int quantity, int page)
        {
            var response = new TableResponse<TransactionResponse>()
            {
                Page = page,
                Show = quantity
            };

            var bankTransactionsQuery = _context.BankTransactions.Where(x => x.ClientId == clientId);
            var walletTransactionsQuery = _context.WalletTransactions.Where(x => x.ClientId == clientId);

            if (startDate.HasValue)
            {
                bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date >= startDate.Value);
                walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                bankTransactionsQuery = bankTransactionsQuery.Where(x => x.Date <= endDate.Value);
                walletTransactionsQuery = walletTransactionsQuery.Where(x => x.Date <= endDate.Value);
            }

            response.Total = bankTransactionsQuery.Count() + walletTransactionsQuery.Count();

            var allTransactions = new List<TransactionResponse>();
            allTransactions.AddRange((bankTransactionsQuery.ToList()).Select(x => new TransactionResponse(x)));
            allTransactions.AddRange((walletTransactionsQuery.ToList()).Select(x => new TransactionResponse(x)));

            response.Data = allTransactions.OrderBy(x => x.Date).Skip(page * quantity).Take(quantity).ToList();

            return response;
        }
    }
}
