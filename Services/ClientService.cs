using AutoMapper;
using SFManagement.Data;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class ClientService : BaseAssetHolderService<Client>
{
    private readonly IMapper _mapper;

    public ClientService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(context,
        httpContextAccessor)
    {
        _mapper = mapper;
    }

    public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request)
    {
        var client = await context.Clients.FindAsync(clientId);

        // client.InitialValue = request.InitialValue ?? client.InitialValue;

        await context.SaveChangesAsync();

        return _mapper.Map<ClientResponse>(client);
    }
}
