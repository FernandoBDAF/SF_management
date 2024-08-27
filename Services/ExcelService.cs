using AutoMapper;
using OfficeOpenXml;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class ExcelService : BaseService<Excel>
    {
        private readonly IMapper _mapper;

        public ExcelService(DataContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<List<WalletTransactionResponse>> ImportBuySellTransactions(ExcelRequest request, WalletTransactionType walletTransactionType)
        {
            var wallet = await context.Wallets.FindAsync(request.WalletId);
            if (wallet == null)
            {
                throw new AppException("Wallet not found");
            }

            var excel = new Excel { CreatedAt = DateTime.Now, WalletId = request.WalletId };

            var rows = this.ReadExcelFile(request.PostFile, new List<(int, string)> { (1, "Nickname"), (2, "Value"), (3, "Wallet"), (6, "CreatedAt"), (8, "Description") });

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
                    excel.WalletTransactions.Add(walletTransaction);
                }
            }

            if (excel.WalletTransactions.Count == 0)
            {
                throw new AppException("No transactions found");
            }

            await context.Excels.AddAsync(excel);
            await context.SaveChangesAsync();

            return _mapper.Map<List<WalletTransactionResponse>>(excel.WalletTransactions);
        }

        public async Task<List<WalletTransactionResponse>> ImportTransferTransactions(ExcelRequest request)
        {
            var wallet = await context.Wallets.FindAsync(request.WalletId);
            if (wallet == null)
            {
                throw new AppException("Wallet not found");
            }

            var excel = new Excel { CreatedAt = DateTime.Now, WalletId = request.WalletId };

            var rows = this.ReadExcelFile(request.PostFile, new List<(int, string)> { (1, "From"), (2, "To"), (3, "CreatedAt"), (4, "Value"), (5, "Description") });

            foreach (var row in rows)
            {
                var walletTransactionValue = Decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value);
                var walletTransactionType = walletTransactionValue > 0 ? WalletTransactionType.Income : WalletTransactionType.Expense;

                var nicknameValue = walletTransactionType == WalletTransactionType.Income ? row.FirstOrDefault(x => x.Name == "From").Value : row.FirstOrDefault(x => x.Name == "To").Value;
                var nickname = context.Nicknames.FirstOrDefault(x => x.Name == nicknameValue && x.WalletId == request.WalletId);

                walletTransactionValue = walletTransactionValue > 0 ? walletTransactionValue : decimal.Negate(walletTransactionValue);

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
                    excel.WalletTransactions.Add(walletTransaction);
                }
            }

            if(excel.WalletTransactions.Count == 0)
            {
                throw new AppException("No transactions found");
            }

            await context.Excels.AddAsync(excel);
            await context.SaveChangesAsync();

            return _mapper.Map<List<WalletTransactionResponse>>(excel.WalletTransactions);
        }

        public List<List<(string Value, string Name)>> ReadExcelFile(IFormFile file, List<(int Column, string Name)> fields)
        {
            var rows = new List<List<(string Value, string Name)>>();

            using (var stream = file.OpenReadStream())
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var rowValues = new List<(string Value, string Name)>();

                        foreach (var field in fields)
                        {
                            var fieldValue = worksheet.Cells[row, field.Column].Text;

                            rowValues.Add((fieldValue, field.Name));
                        }

                        rows.Add(rowValues);
                    }
                }
            }

            return rows;
        }
    }
}
