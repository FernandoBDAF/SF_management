using SFManagement.Application.Services.Transactions;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.Services.AssetHolders;
using SFManagement.Application.Services.Assets;
﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Application.Services;
using SFManagement.Application.DTOs;

namespace SFManagement.Api.Controllers.v1.AssetHolders;

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
