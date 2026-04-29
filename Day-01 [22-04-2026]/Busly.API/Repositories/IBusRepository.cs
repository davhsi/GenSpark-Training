using Busly.API.Models;

namespace Busly.API.Repositories;

public interface IBusRepository
{
    Task<BusLayout> CreateLayoutAsync(BusLayout layout);
    Task<List<BusLayout>> GetLayoutsByOperatorAsync(Guid operatorId);
    Task<BusLayout?> GetLayoutByIdAsync(Guid id);
    Task RemoveLayoutAsync(Guid id);
    Task<bool> IsLayoutInUseAsync(Guid layoutId);
    Task<Bus> CreateBusAsync(Bus bus);
    Task<Bus?> GetBusByIdAsync(Guid id);
    Task<List<Bus>> GetBusesByOperatorAsync(Guid operatorId);
    Task<BusStop> AddBusStopAsync(BusStop busStop);
    Task<BusStop?> GetBusStopByIdAsync(Guid stopId);
    Task RemoveBusStopAsync(Guid stopId);
    Task UpdateBusPriceAsync(Guid busId, decimal newPrice);
    Task UpdateBusStaffAsync(Guid busId, string? dName, string? dPhone, string? cName, string? cPhone);
    Task UpdateBusStatusAsync(Guid busId, string status);
    Task<List<Booking>> GetConfirmedFutureBookingsByBusAsync(Guid busId);
    Task CreateOperatingDaysAsync(List<BusOperatingDay> operatingDays);
    Task<List<BusOperatingDay>> GetOperatingDaysByBusAsync(Guid busId);
    Task UpdateOperatingDaysAsync(Guid busId, List<BusOperatingDay> operatingDays);
}
