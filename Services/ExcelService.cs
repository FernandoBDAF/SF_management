using System.Globalization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public ExcelService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
            _mapper = mapper;
        }

        public override async Task<Excel?> Get(Guid id) => await _entity.Include(x => x.WalletTransactions)
                                                                        .FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);

        public async Task<List<WalletTransactionResponse>> ImportBuySellTransactions(ExcelRequest request, WalletTransactionType walletTransactionType)
        {
            var manager = await context.Managers.FindAsync(request.ManagerId);
            if (manager == null)
            {
                throw new AppException("Manager not found");
            }

            var excel = new Excel
            {
                CreatedAt = DateTime.Now,
                ManagerId = request.ManagerId,
                FileName = request.PostFile.FileName,
                FileType = walletTransactionType == WalletTransactionType.Income ? "Compra" : "Venda"
            };

            var rows = this.ReadExcelFile(request.PostFile, new List<(int, string)> { (1, "Nickname"), (2, "Value"), (3, "Wallet"), (6, "CreatedAt"), (8, "Description") });

            foreach (var row in rows)
            {
                var nicknameValue = row.FirstOrDefault(x => x.Name == "Nickname").Value;
                var createdAtValue = row.FirstOrDefault(x => x.Name == "CreatedAt").Value;
                var walletTransaction = new WalletTransaction
                {
                    Date = DateTime.Parse(row.FirstOrDefault(x => x.Name == "CreatedAt").Value),
                    Coins = Decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value),
                    Description = row.FirstOrDefault(x => x.Name == "Description").Value,
                    WalletTransactionType = walletTransactionType,
                    ExcelNickname = nicknameValue
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
            var manager = await context.Managers.FindAsync(request.ManagerId);
            if (manager == null)
            {
                throw new AppException("Manager not found");
            }

            var excel = new Excel
            {
                CreatedAt = DateTime.Now,
                ManagerId = request.ManagerId,
                FileName = request.PostFile.FileName,
                FileType = "Transferência"
            };

            var rows = this.ReadExcelFile(request.PostFile, new List<(int, string)> { (1, "From"), (2, "To"), (3, "CreatedAt"), (4, "Value"), (5, "Description") });

            foreach (var row in rows)
            {
                var walletTransactionValue = Decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value);
                var walletTransactionType = walletTransactionValue > 0 ? WalletTransactionType.Income : WalletTransactionType.Expense;

                walletTransactionValue = walletTransactionValue > 0 ? walletTransactionValue : decimal.Negate(walletTransactionValue);

                var walletTransaction = new WalletTransaction
                {
                    Date = DateTime.Parse(row.FirstOrDefault(x => x.Name == "CreatedAt").Value),
                    Coins = Decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value),
                    Description = row.FirstOrDefault(x => x.Name == "Description").Value,
                    WalletTransactionType = walletTransactionType,
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

        public async Task<List<WalletTransactionResponse>> Reconciliation(Guid from, Guid to)
        {
            var list = new List<WalletTransaction>();

            var walletTransactionFrom = context.WalletTransactions.FirstOrDefault(x => x.Id == from);
            if (walletTransactionFrom == null)
            {
                throw new AppException("From not found");
            }
            if (walletTransactionFrom.WalletId != null)
            {
                throw new AppException("Já vinculado com algum lançamento manual");
            }
            if (walletTransactionFrom.NicknameId != null)
            {
                throw new AppException("Já vinculado com algum lançamento manual");
            }

            var walletTransactionTo = context.WalletTransactions.FirstOrDefault(x => x.Id == to);
            if (walletTransactionTo == null)
            {
                throw new AppException("To not found");
            }
            if (walletTransactionTo.LinkedToId.HasValue)
            {
                throw new AppException("Esta transação manual já foi vinculada a uma transação OFX.");
            }

            if (walletTransactionTo.WalletTransactionType != walletTransactionFrom.WalletTransactionType)
            {
                throw new AppException("Transações de tipos diferentes.");
            }

            if (walletTransactionTo.Coins != walletTransactionFrom.Coins)
            {
                throw new AppException("Transações de valores de fichas diferentes.");
            }

            walletTransactionTo.LinkedToId = walletTransactionFrom.Id;
            walletTransactionTo.ApprovedAt = DateTime.Now;

            context.WalletTransactions.Update(walletTransactionTo);

            walletTransactionFrom.ApprovedAt = DateTime.Now;
            walletTransactionFrom.NicknameId = walletTransactionTo.NicknameId;
            walletTransactionFrom.WalletId = walletTransactionTo.WalletId;

            context.WalletTransactions.Update(walletTransactionTo);

            list.AddRange(walletTransactionFrom, walletTransactionTo);

            await context.SaveChangesAsync();

            return _mapper.Map<List<WalletTransactionResponse>>(list);
        }
    }
}
