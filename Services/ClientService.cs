using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class ClientService : BaseService<Client>
{
    private readonly IMapper _mapper;

    public ClientService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(context,
        httpContextAccessor)
    {
        _mapper = mapper;
    }

    public async Task<BalanceResponse> GetBalance(Guid clientId, DateTime? date)
    {
        var now = DateTime.Now;
        if (!date.HasValue || date.Value.Year == 1) date = now;
        var client = await context.Clients.Include(x => x.BankTransactions)
            .Include(x => x.WalletTransactions)
            .Include(x => x.InternalTransactions)
            .FirstOrDefaultAsync(x => x.Id == clientId);

        return new BalanceResponse(client, date);
    }

    public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request)
    {
        var client = await context.Clients.FindAsync(clientId);

        client.InitialValue = request.InitialValue ?? client.InitialValue;

        await context.SaveChangesAsync();

        return _mapper.Map<ClientResponse>(client);
    }
}