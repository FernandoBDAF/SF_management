using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ClosingWalletController : BaseApiController<ClosingWallet, ClosingWalletRequest, ClosingWalletResponse>
{
    private readonly ClosingWalletService _closingWalletService;
    private readonly IMapper _mapper;

    public ClosingWalletController(BaseService<ClosingWallet> service, IMapper mapper,
        ClosingWalletService closingWalletService) : base(service, mapper)
    {
        _closingWalletService = closingWalletService;
        _mapper = mapper;
    }

    [HttpGet]
    [Route("closing-manager/{closingManagerId}")]
    public async Task<List<ClosingWalletResponse>> GetByClosingManagerId(Guid closingManagerId)
    {
        return _mapper.Map<List<ClosingWalletResponse>>(
            await _closingWalletService.GetByClosingManagerId(closingManagerId));
    }
}