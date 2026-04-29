using Busly.API.DTOs.Admin;

namespace Busly.API.Services;

public interface IAdminService
{
    Task<RouteDto> CreateRouteAsync(CreateRouteRequest request, Guid adminId);
    Task<List<RouteDto>> GetAllRoutesAsync();
    Task<List<RouteDto>> GetActiveRoutesAsync();
    Task ToggleRouteAsync(Guid routeId, Guid adminId);

    Task<List<OperatorDto>> GetPendingOperatorsAsync();
    Task<List<OperatorDto>> GetAllOperatorsAsync();
    Task ApproveOperatorAsync(Guid operatorId, Guid adminId);
    Task RejectOperatorAsync(Guid operatorId, Guid adminId);
    Task ToggleOperatorAsync(Guid operatorId, Guid adminId);

    Task<List<BusDto>> GetPendingBusesAsync();
    Task<List<BusDto>> GetAllBusesAsync();
    Task<List<BusDto>> GetBusesByOperatorAsync(Guid operatorId);
    Task ApproveBusAsync(Guid busId, Guid adminId);
    Task RejectBusAsync(Guid busId, Guid adminId);
    Task ToggleBusStatusAsync(Guid busId, Guid adminId);

    Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync();
    Task<List<OperatorRevenueDto>> GetOperatorRevenueAsync();

    Task PublishTcVersionAsync(CreateTcRequest request, Guid adminId);
    Task<List<TcVersionDto>> GetAllTcVersionsAsync();
    Task<TcVersionDto?> GetCurrentTcAsync();
    Task<List<AuditLogDto>> GetAuditLogsAsync();
}
