using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ClosingWalletController : BaseApiController<ClosingWallet, ClosingWalletRequest, ClosingWalletResponse>
    {
        private readonly ClosingWalletService _closingWalletService;
        private readonly IMapper _mapper;

        public ClosingWalletController(BaseService<ClosingWallet> service, IMapper mapper, ClosingWalletService closingWalletService) : base(service, mapper)
        {
            _closingWalletService = closingWalletService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("closing-manager/{closingManagerId}")]
        public async Task<List<ClosingWalletResponse>> GetByClosingManagerId(Guid closingManagerId) => _mapper.Map<List<ClosingWalletResponse>>(await _closingWalletService.GetByClosingManagerId(closingManagerId));
    }
}
