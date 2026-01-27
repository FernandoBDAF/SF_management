using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Support;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Exceptions;
using SFManagement.Domain.Interfaces;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.AssetHolders;

public class ClientService : BaseAssetHolderService<Client>
{
    public ClientService(
        DataContext context, 
        IHttpContextAccessor httpContextAccessor,
        IAssetHolderDomainService domainService,
        ReferralService referralService,
        InitialBalanceService initialBalanceService) 
        : base(context, httpContextAccessor, domainService, referralService, initialBalanceService)
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
            HasActiveAssetPools = baseStatistics.HasActiveAssetPools,
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
    public bool HasActiveAssetPools { get; set; }
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