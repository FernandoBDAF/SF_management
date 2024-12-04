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
        private readonly IMapper _mapper;

        public WalletTransactionService(DataContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<WalletTransactionResponse> ApproveTransaction(Guid walletTransactionId, WalletTransactionApproveRequest model)
        {
            var walletTransaction = await base.Get(walletTransactionId);
            if (walletTransaction == null)
            {
                throw new AppException("Wallet transaction not found");
            }

            walletTransaction.ApprovedAt = DateTime.Now;

            if (!model.TagId.HasValue && !model.ClientId.HasValue)
            {
                throw new AppException($"Need send TagId or ClientId.");
            }

            walletTransaction.TagId = model.TagId;
            walletTransaction.ClientId = model.ClientId;

            context.WalletTransactions.Update(walletTransaction);
            await context.SaveChangesAsync();

            return _mapper.Map<WalletTransactionResponse>(walletTransaction);
        }
    }
}
