using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Entities;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class InitialBalanceController(InitialBalanceService service, IMapper mapper)
    : BaseApiController<InitialBalance, InitialBalanceRequest, InitialBalanceResponse>(service, mapper)
{
    private readonly InitialBalanceService _initialBalanceService = service;
}