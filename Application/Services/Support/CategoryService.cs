using SFManagement.Application.DTOs.Support;
using SFManagement.Application.Services.Base;
using SFManagement.Infrastructure.Data;
﻿using Microsoft.EntityFrameworkCore;
using SFManagement.Infrastructure.Data;
using SFManagement.Domain.Entities.Support;
using SFManagement.Application.DTOs;

namespace SFManagement.Application.Services.Support;

public class CategoryService : BaseService<Category>
{
    public CategoryService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    // public async Task<BalanceResponse> GetBalance(Guid financialBehaviorId)
    // {
    //     var financialBehavior = await context.FinancialBehaviors.Include(x => x.BankTransactions).Include(x => x.WalletTransactions)
    //         // .Include(x => x.InternalTransactions)
    //         .FirstOrDefaultAsync(x => x.Id == financialBehaviorId);
    //
    //     return new BalanceResponse(financialBehavior);
    // }

    public override async Task<List<Category>> List()
    {
        var query = await context.Categories.Where(x => !x.CategoryId.HasValue).ToListAsync();

        foreach (var financialBehavior in query) await GetChildren(financialBehavior);

        return query;
    }

    private async Task<List<Category>> GetChildren(Category category)
    {
        var chds = await context.Categories
            .Where(x => x.CategoryId == category.Id && x.DeletedAt == null)
            .ToListAsync();

        foreach (var chd in chds) await GetChildren(chd);

        // financialBehavior.Children.AddRange(chds);

        return null;
    }
}