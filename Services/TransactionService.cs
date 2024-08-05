using SFManagement.Data;
using SFManagement.ViewModels;
using System.Data.Entity;

namespace SFManagement.Services
{
    public class TransactionService
    {
        private readonly DataContext _context;

        public TransactionService(DataContext context)
        {
            _context = context;
        }

        public async Task<TableResponse<TransactionResponse>> GetTransactions(Guid clientId, DateTime? startDate, DateTime? endDate, int quantity, int page)
        {
            var response = new TableResponse<TransactionResponse>()
            {
                Page = page,
                Show = quantity
            };

            var query = _context.BankTransactions.Where(x => x.ClientId == clientId);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.Date <= endDate.Value);
            }

            response.Total = await query.CountAsync();

            query = query.Skip(page * quantity).Take(quantity);

            response.Data.AddRange((await query.ToListAsync()).Select(x => new TransactionResponse(x)));

            return response;
        }
    }
}
