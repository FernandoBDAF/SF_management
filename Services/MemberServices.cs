using AutoMapper;
using SFManagement.Data;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class MemberService : BaseService<Member>
{
    private readonly IMapper _mapper;

    public MemberService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(context,
        httpContextAccessor)
    {
        _mapper = mapper;
    }
}