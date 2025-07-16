using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Controllers;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BankController : BaseAssetHolderController<Bank, BankRequest, BankResponse>
{
    private readonly BankService _bankService;
    private readonly WalletIdentifierService _walletIdentifierService;

    public BankController(BankService service, IMapper mapper, ILogger<BaseAssetHolderController<Bank, BankRequest, BankResponse>> logger,
        WalletIdentifierService walletIdentifierService) 
        : base(service, walletIdentifierService, mapper, logger)
    {
        _bankService = service;
    }

    /// <summary>
    /// Creates an entity from request - Bank-specific implementation
    /// </summary>
    protected override async Task<Bank> CreateEntityFromRequest(BankRequest request)
    {
        return await _bankService.AddFromRequest(request);
    }

    /// <summary>
    /// Updates an entity from request - Bank-specific implementation
    /// </summary>
    protected override async Task<Bank> UpdateEntityFromRequest(Guid id, BankRequest request)
    {
        return await _bankService.UpdateFromRequest(id, request);
    }
}
