using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Interfaces;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class BankService : BaseAssetHolderService<Bank>
{
    public BankService(
        DataContext context, 
        IHttpContextAccessor httpContextAccessor,
        IAssetHolderDomainService domainService,
        ReferralService referralService,
        InitialBalanceService initialBalanceService) 
        : base(context, httpContextAccessor, domainService, referralService, initialBalanceService)
    {
    }

    /// <summary>
    /// Creates a new bank with comprehensive validation
    /// </summary>
    public async Task<Bank> AddFromRequest(BankRequest request)
    {
        return await base.AddFromRequest(
            request,
            baseAssetHolder => new Bank
            {
                BaseAssetHolderId = baseAssetHolder.Id,
                Code = request.Code
            },
            _domainService.ValidateBankCreation
        );
    }

    /// <summary>
    /// Updates a bank with validation
    /// </summary>
    public async Task<Bank> UpdateFromRequest(Guid bankId, BankRequest request)
    {
        return await base.UpdateFromRequest(
            bankId,
            request,
            (bank, req) => bank.Code = req.Code,
            _domainService.ValidateBankCreation
        );
    }
}