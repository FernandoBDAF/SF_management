using SFManagement.Application.DTOs.Finance;

namespace SFManagement.Application.Services.Finance;

public interface IProfitCalculationService
{
    Task<ProfitSummary> GetProfitSummary(DateTime startDate, DateTime endDate, Guid? managerId = null);
    Task<List<ProfitByManager>> GetProfitByManager(DateTime startDate, DateTime endDate);
    Task<List<ProfitBySource>> GetProfitBySource(DateTime startDate, DateTime endDate);
    Task<DirectIncomeDetailsResponse> GetDirectIncomeDetails(DateTime startDate, DateTime endDate);
}
