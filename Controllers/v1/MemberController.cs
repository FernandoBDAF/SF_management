using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MemberController(MemberService service, IMapper mapper)
    : BaseApiController<Member, MemberRequest, MemberResponse>(service, mapper)
{
    private readonly MemberService _initialBalanceService = service;
}