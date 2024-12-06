using AutoMapper;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services
{
    public class WalletTransactionService : BaseService<WalletTransaction>
    {
        private readonly IMapper _mapper;

        public WalletTransactionService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
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
            walletTransaction.NicknameId = model.NicknameId;
            walletTransaction.TagId = model.TagId;
            walletTransaction.ClientId = model.ClientId;

            context.WalletTransactions.Update(walletTransaction);
            await context.SaveChangesAsync();

            return _mapper.Map<WalletTransactionResponse>(walletTransaction);
        }
    }
}
