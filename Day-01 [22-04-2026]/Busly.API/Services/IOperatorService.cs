using Busly.API.DTOs.Operator;

namespace Busly.API.Services;

public interface IOperatorService
{
    Task<LayoutDto> CreateLayoutAsync(CreateLayoutRequest request, Guid operatorId);
    Task<List<LayoutDto>> GetLayoutsAsync(Guid operatorId);
    Task RemoveLayoutAsync(Guid layoutId, Guid operatorId);
    Task<BusDetailDto> RegisterBusAsync(RegisterBusRequest request, Guid operatorId);
    Task AddBusStopAsync(Guid busId, AddBusStopRequest request, Guid operatorId);
    Task RemoveBusStopAsync(Guid stopId, Guid operatorId);
    Task UpdateBusPriceAsync(Guid busId, UpdatePriceRequest request, Guid operatorId);
    Task UpdateBusStaffAsync(Guid busId, UpdateStaffRequest request, Guid operatorId);
    Task UpdateOperatingDaysAsync(UpdateOperatingDaysRequest request, Guid operatorId);
    Task DisableBusAsync(Guid busId, Guid operatorId);
    Task RemoveBusAsync(Guid busId, Guid operatorId);
    Task<List<BusDetailDto>> GetBusesAsync(Guid operatorId);
    Task<OperatorProfileDto> GetProfileAsync(Guid operatorId);
    Task<OperatorProfileDto> GetProfileWithoutApprovalCheckAsync(Guid operatorId);
}
