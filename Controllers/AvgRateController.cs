using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [Authorize]
    public class AvgRateController : BaseApiController<AvgRate, AvgRateRequest, AvgRateResponse>
    {
        private readonly AvgRateService _avgService;
        private readonly WalletTransactionService _walletTransactionService;

        public AvgRateController(BaseService<AvgRate> service, IMapper mapper, AvgRateService avgService, WalletTransactionService walletTransactionService) : base(service, mapper)
        {
            _avgService = avgService;
            _walletTransactionService = walletTransactionService;
        }

        [HttpPut]
        [Route("reset/{managerId}")]
        public async Task Reset(Guid managerId)
        {
            await _avgService.Reset(managerId);
            await _walletTransactionService.SetExchangeRate(managerId);
            await _walletTransactionService.CalcProfits(managerId);
        }

        [HttpPut]
        [Route("{managerId}/{referenceDate}")]
        public async Task Calc(Guid managerId, DateTime referenceDate)
        {
            await _avgService.Calc(managerId, referenceDate);
            await _walletTransactionService.SetExchangeRate(managerId);
            await _walletTransactionService.CalcProfits(managerId);
        }
    }
}
