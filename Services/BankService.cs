using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class BankService : BaseAssetHolderService<Bank>
{

    public BankService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    // Method to handle BankRequest and create both BaseAssetHolder and Bank
    public async Task<Bank> AddFromRequest(BankRequest request)
    {
        // Create BaseAssetHolder using helper method
        var baseAssetHolder = await CreateBaseAssetHolder(
            request.Name
        );

        // Create Bank using the BaseAssetHolder's ID
        var bank = new Bank
        {
            BaseAssetHolderId = baseAssetHolder.Id,
            Code = int.TryParse(request.Code, out int code) ? code : 0
        };

        // Use base service to add Bank (handles audit automatically)
        var result = await base.Add(bank);

        // Return the bank with BaseAssetHolder included
        return await context.Banks
            .Include(b => b.BaseAssetHolder)
            .FirstOrDefaultAsync(b => b.Id == result.Id);
    }
}