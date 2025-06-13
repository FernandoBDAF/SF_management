using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Services;
// using SFManagement.ViewModels;
//
// namespace SFManagement.Controllers.v1;
//
// [ApiController]
// [Route("api/v{verion:apiVersion}/[controller]")]
// [ApiVersion("1.0")]
// public class AvgRateController : BaseApiController<AvgRate, AvgRateRequest, AvgRateResponse>
// {
//     private readonly AvgRateService _avgService;
//     private readonly WalletTransactionService _walletTransactionService;
//
//     public AvgRateController(BaseService<AvgRate> service, IMapper mapper, AvgRateService avgService,
//         WalletTransactionService walletTransactionService) : base(service, mapper)
//     {
//         _avgService = avgService;
//         _walletTransactionService = walletTransactionService;
//     }
//
//     [HttpGet]
//     [Route("reset/{managerId}")]
//     public async Task Reset(Guid managerId)
//     {
//         await _avgService.Reset(managerId);
//         await _walletTransactionService.SetExchangeRate(managerId);
//         await _walletTransactionService.CalcProfits(managerId);
//     }
//
//     [HttpGet]
//     [Route("{managerId}/{referenceDate}")]
//     public async Task Calc(Guid managerId, DateTime referenceDate)
//     {
//         await _avgService.Calc(managerId, referenceDate);
//         await _walletTransactionService.SetExchangeRate(managerId);
//         await _walletTransactionService.CalcProfits(managerId);
//     }
// }