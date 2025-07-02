using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Entities;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class MemberService : BaseAssetHolderService<Member>
{
    public MemberService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    // Method to handle MemberRequest and create both BaseAssetHolder and Member
    public async Task<Member> AddFromRequest(MemberRequest request)
    {
        // Create BaseAssetHolder using helper method
        var baseAssetHolder = await CreateBaseAssetHolder(
            request.Name, 
            request.Email, 
            request.Cpf, 
            request.Cnpj
        );

        // Create Member using the BaseAssetHolder's ID
        var member = new Member
        {
            BaseAssetHolderId = baseAssetHolder.Id,
            Share = request.Share ?? 0.0,
            Birthday = request.Birthday
        };

        // Use base service to add Member (handles audit automatically)
        var result = await base.Add(member);

        // Return the member with BaseAssetHolder included
        return await context.Members
            .Include(m => m.BaseAssetHolder)
            .FirstOrDefaultAsync(m => m.Id == result.Id);
    }
}