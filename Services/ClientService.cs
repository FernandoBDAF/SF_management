using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Exceptions;
using SFManagement.Interfaces;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class ClientService : BaseAssetHolderService<Client>
{
    public ClientService(
        DataContext context, 
        IHttpContextAccessor httpContextAccessor,
        IAssetHolderDomainService domainService) 
        : base(context, httpContextAccessor, domainService)
    {
    }

    /// <summary>
    /// Creates a new client with comprehensive validation
    /// </summary>
    public async Task<Client> AddFromRequest(ClientRequest request)
    {
        return await base.AddFromRequest(
            request,
            baseAssetHolder => new Client
            {
                BaseAssetHolderId = baseAssetHolder.Id,
                Birthday = request.Birthday
            },
            _domainService.ValidateClientCreation
        );
    }

    /// <summary>
    /// Updates a client with validation
    /// </summary>
    public async Task<Client> UpdateFromRequest(Guid clientId, ClientRequest request)
    {
        return await base.UpdateFromRequest(
            clientId,
            request,
            (client, req) => client.Birthday = req.Birthday,
            _domainService.ValidateClientCreation
        );
    }

    /// <summary>
    /// Gets client statistics with client-specific properties
    /// </summary>
    public async Task<ClientStatistics> GetClientStatistics(Guid clientId)
    {
        var baseStatistics = await GetAssetHolderStatistics(clientId);
        var client = await Get(clientId);
        
        return new ClientStatistics
        {
            ClientId = baseStatistics.EntityId,
            BaseAssetHolderId = baseStatistics.BaseAssetHolderId,
            HasActiveTransactions = baseStatistics.HasActiveTransactions,
            TotalBalance = baseStatistics.TotalBalance,
            HasActiveAssetWallets = baseStatistics.HasActiveAssetWallets,
            Age = client?.Age,
            CanBeDeleted = baseStatistics.CanBeDeleted
        };
    }
}

/// <summary>
/// Client-specific statistics data transfer object
/// </summary>
public class ClientStatistics
{
    public Guid ClientId { get; set; }
    public Guid BaseAssetHolderId { get; set; }
    public bool HasActiveTransactions { get; set; }
    public decimal TotalBalance { get; set; }
    public bool HasActiveAssetWallets { get; set; }
    public int? Age { get; set; }
    public bool CanBeDeleted { get; set; }
}

// public async Task<ClientResponse> UpdateInitialValue(Guid clientId, ClientRequest request)
    // {
    //     var client = await context.Clients.FindAsync(clientId);

    //     // client.InitialValue = request.InitialValue ?? client.InitialValue;

    //     await context.SaveChangesAsync();

    //     return _mapper.Map<ClientResponse>(client);
    // }