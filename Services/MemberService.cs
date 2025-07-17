using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Interfaces;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class MemberService : BaseAssetHolderService<Member>
{
    public MemberService(
        DataContext context, 
        IHttpContextAccessor httpContextAccessor,
        IAssetHolderDomainService domainService,
        ReferralService referralService,
        InitialBalanceService initialBalanceService) 
        : base(context, httpContextAccessor, domainService, referralService, initialBalanceService)
    {
    }

    /// <summary>
    /// Creates a new member with comprehensive validation
    /// </summary>
    public async Task<Member> AddFromRequest(MemberRequest request)
    {
        return await base.AddFromRequest(
            request,
            baseAssetHolder => new Member
            {
                BaseAssetHolderId = baseAssetHolder.Id,
                Share = request.Share ?? 0,
                Birthday = request.Birthday
            },
            _domainService.ValidateMemberCreation
        );
    }

    /// <summary>
    /// Updates a member with validation
    /// </summary>
    public async Task<Member> UpdateFromRequest(Guid memberId, MemberRequest request)
    {
        return await base.UpdateFromRequest(
            memberId,
            request,
            (member, req) => 
            {
                member.Share = req.Share ?? 0;
                member.Birthday = req.Birthday;
            },
            _domainService.ValidateMemberCreation
        );
    }

    /// <summary>
    /// Gets member statistics with member-specific properties
    /// </summary>
    public async Task<MemberStatistics> GetMemberStatistics(Guid memberId)
    {
        var baseStatistics = await GetAssetHolderStatistics(memberId);
        var member = await Get(memberId);
        
        return new MemberStatistics
        {
            MemberId = baseStatistics.EntityId,
            BaseAssetHolderId = baseStatistics.BaseAssetHolderId,
            HasActiveTransactions = baseStatistics.HasActiveTransactions,
            TotalBalance = baseStatistics.TotalBalance,
            HasActiveAssetPools = baseStatistics.HasActiveAssetPools,
            Share = member?.Share ?? 0,
            IsActiveShare = member?.IsActiveShare ?? false,
            CanBeDeleted = baseStatistics.CanBeDeleted
        };
    }
}

/// <summary>
/// Member-specific statistics data transfer object
/// </summary>
public class MemberStatistics
{
    public Guid MemberId { get; set; }
    public Guid BaseAssetHolderId { get; set; }
    public bool HasActiveTransactions { get; set; }
    public decimal TotalBalance { get; set; }
    public bool HasActiveAssetPools { get; set; }
    public decimal Share { get; set; }
    public bool IsActiveShare { get; set; }
    public bool CanBeDeleted { get; set; }
}