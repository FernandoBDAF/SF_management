using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.ViewModels;
using System.Data.Entity;

namespace SFManagement.Services
{
    public class WalletTransactionService : BaseService<WalletTransaction>
    {
        private readonly ExcelService _excelService;
        private readonly IMapper _mapper;

        public WalletTransactionService(DataContext context, ExcelService excelService, IMapper mapper) : base(context)
        {
            _excelService = excelService;
            _mapper = mapper;
        }

        public async Task<List<WalletTransactionResponse>> ImportBuySellTransactions(ImportBuySellTransactionsRequest request, WalletTransactionType walletTransactionType)
        {
            var wallet = await context.Wallets.FindAsync(request.WalletId);
            if (wallet == null)
            {
                throw new Exception("Wallet not found");
            }

            var walletTransactions = new List<WalletTransaction>();

            var rows = _excelService.ReadExcelFile(request.File, new List<(int, string)> { (1, "Nickname"), (2, "Value"), (3, "Wallet"), (6, "CreatedAt"), (8, "Description") });

            foreach (var row in rows)
            {
                var nicknameValue = row.FirstOrDefault(x => x.Name == "Nickname").Value;

                var nickname = context.Nicknames.FirstOrDefault(x => x.Name == nicknameValue && x.WalletId == request.WalletId);

                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Date = DateTime.Parse(row.FirstOrDefault(x => x.Name == "CreatedAt").Value),
                    Value = Decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value),
                    Description = row.FirstOrDefault(x => x.Name == "Description").Value,
                    WalletTransactionType = walletTransactionType,
                    NicknameId = nickname?.Id
                };

                if (!context.WalletTransactions.Any(x => x.Date == walletTransaction.Date && x.Value == walletTransaction.Value && x.WalletTransactionType == walletTransaction.WalletTransactionType && x.WalletId == walletTransaction.WalletId))
                {
                    walletTransactions.Add(walletTransaction);
                }
            }

            await context.WalletTransactions.AddRangeAsync(walletTransactions);
            await context.SaveChangesAsync();

            return _mapper.Map<List<WalletTransactionResponse>>(walletTransactions);
        }

        public async Task<List<WalletTransactionResponse>> ImportTransferTransactions(ImportTransferTransactionRequest request)
        {
            var wallet = await context.Wallets.FindAsync(request.WalletId);
            if (wallet == null)
            {
                throw new Exception("Wallet not found");
            }

            var walletTransactions = new List<WalletTransaction>();

            var rows = _excelService.ReadExcelFile(request.File, new List<(int, string)> { (1, "From"), (2, "To"), (3, "CreatedAt"), (4, "Value"), (5, "Description") });

            foreach (var row in rows)
            {
                var walletTransactionValue = Decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value);
                var walletTransactionType = walletTransactionValue > 0 ? WalletTransactionType.Income : WalletTransactionType.Expense;
                
                var nicknameValue = walletTransactionType == WalletTransactionType.Income ? row.FirstOrDefault(x => x.Name == "From").Value : row.FirstOrDefault(x => x.Name == "To").Value;
                var nickname = context.Nicknames.FirstOrDefault(x => x.Name == nicknameValue && x.WalletId == request.WalletId);

                walletTransactionValue = walletTransactionValue >  0 ? walletTransactionValue : decimal.Negate(walletTransactionValue);

                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Date = DateTime.Parse(row.FirstOrDefault(x => x.Name == "CreatedAt").Value),
                    Value = Decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value),
                    Description = row.FirstOrDefault(x => x.Name == "Description").Value,
                    WalletTransactionType = walletTransactionType,
                    NicknameId = nickname?.Id
                };

                if (!context.WalletTransactions.Any(x => x.Date == walletTransaction.Date && x.Value == walletTransaction.Value && x.WalletTransactionType == walletTransaction.WalletTransactionType && x.WalletId == walletTransaction.WalletId))
                {
                    walletTransactions.Add(walletTransaction);
                }
            }

            await context.WalletTransactions.AddRangeAsync(walletTransactions);
            await context.SaveChangesAsync();

            return _mapper.Map<List<WalletTransactionResponse>>(walletTransactions);
        }
    }
}
