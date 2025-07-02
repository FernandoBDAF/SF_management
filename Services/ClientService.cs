using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class ClientService : BaseAssetHolderService<Client>
{
    public ClientService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    // Method to handle ClientRequest and create both BaseAssetHolder and Client
    public async Task<Client> AddFromRequest(ClientRequest request)
    {
        // Create BaseAssetHolder using helper method
        var baseAssetHolder = await CreateBaseAssetHolder(
            request.Name, 
            request.Email, 
            request.Cpf, 
            request.Cnpj
        );

        // Create Client using the BaseAssetHolder's ID
        var client = new Client
        {
            BaseAssetHolderId = baseAssetHolder.Id,
            Birthday = request.Birthday
        };

        // Use base service to add Client (handles audit automatically)
        var result = await base.Add(client);

        // Return the client with BaseAssetHolder included
        return await context.Clients
            .Include(c => c.BaseAssetHolder)
            .FirstOrDefaultAsync(c => c.Id == result.Id);
    }

    // public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request)
    // {
    //     var client = await context.Clients.FindAsync(clientId);

    //     // client.InitialValue = request.InitialValue ?? client.InitialValue;

    //     await context.SaveChangesAsync();

    //     return _mapper.Map<ClientResponse>(client);
    // }
}
