using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Transactions;
using SFManagement.Domain.Exceptions;

namespace SFManagement.Api.Controllers.v1.Transactions;

/// <summary>
/// Unified transfer operations between asset holders.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class TransferController : ControllerBase
{
    private readonly TransferService _transferService;
    private readonly ILogger<TransferController> _logger;

    public TransferController(TransferService transferService, ILogger<TransferController> logger)
    {
        _transferService = transferService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a transfer between any two asset holders.
    /// </summary>
    /// <remarks>
    /// Supports both simple mode (auto-select wallets) and advanced mode (specify wallet IDs).
    /// 
    /// Transaction Types:
    /// - Fiat assets (BrazilianReal=21, USDollar=22) -> FiatAssetTransaction
    /// - Digital assets (PokerStars=101, Bitcoin=201, etc.) -> DigitalAssetTransaction
    /// </remarks>
    [HttpPost]
    // [Authorize(Policy = "Permission:create:transactions")]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Transfer: {Sender} -> {Receiver}, AssetType={AssetType}, Amount={Amount}",
                request.SenderAssetHolderId, request.ReceiverAssetHolderId,
                request.AssetType, request.Amount);

            var response = await _transferService.TransferAsync(request);

            _logger.LogInformation(
                "Transfer completed: TxnId={TxnId}, Type={Type}, Internal={IsInternal}",
                response.TransactionId, response.EntityType, response.IsInternalTransfer);

            return Ok(response);
        }
        catch (WalletMissingException ex)
        {
            _logger.LogInformation(
                "Transfer requires wallet creation: SenderMissing={SenderMissing}, ReceiverMissing={ReceiverMissing}",
                ex.Details.SenderWalletMissing, ex.Details.ReceiverWalletMissing);

            return BadRequest(new ProblemDetails
            {
                Title = "Wallet Creation Required",
                Detail = ex.Details.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions =
                {
                    ["errorCode"] = ex.Details.Code,
                    ["walletDetails"] = ex.Details
                }
            });
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: {Code} - {Message}", ex.Code, ex.Message);

            return BadRequest(new ProblemDetails
            {
                Title = "Transfer Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions =
                {
                    ["errorCode"] = ex.Code,
                    ["errors"] = new[] { new TransferError { Code = ex.Code, Message = ex.Message } }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during transfer");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Gets a transfer by ID.
    /// </summary>
    [HttpGet("{id}")]
    // [Authorize(Policy = "Permission:read:transactions")]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransfer(Guid id, [FromQuery] string entityType = "fiat")
    {
        var response = await _transferService.GetTransferAsync(id, entityType.ToLowerInvariant() == "fiat");

        if (response == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Transfer '{id}' not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(response);
    }
}

