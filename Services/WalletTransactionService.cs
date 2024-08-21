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
    }
}
