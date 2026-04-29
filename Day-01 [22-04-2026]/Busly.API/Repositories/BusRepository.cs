using Busly.API.Data;
using Busly.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class BusRepository : IBusRepository
{
    private readonly AppDbContext _db;

    public BusRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BusLayout> CreateLayoutAsync(BusLayout layout)
    {
        _db.BusLayouts.Add(layout);
        await _db.SaveChangesAsync();
        return layout;
    }

    // Filter layouts by operatorId for data isolation
    public async Task<List<BusLayout>> GetLayoutsByOperatorAsync(Guid operatorId)
    {
        return await _db.BusLayouts
            .Include(l => l.Seats)
            .Where(l => l.OperatorId == operatorId)
            .ToListAsync();
    }

    public async Task<BusLayout?> GetLayoutByIdAsync(Guid id)
    {
        return await _db.BusLayouts.Include(l => l.Seats).FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task RemoveLayoutAsync(Guid id)
    {
        var layout = await _db.BusLayouts.FindAsync(id);
        if (layout is not null)
        {
            // Remove seats first due to FK
            var seats = _db.Seats.Where(s => s.LayoutId == id);
            _db.Seats.RemoveRange(seats);
            
            _db.BusLayouts.Remove(layout);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsLayoutInUseAsync(Guid layoutId)
    {
        return await _db.Buses.AnyAsync(b => b.LayoutId == layoutId);
    }

    public async Task<Bus> CreateBusAsync(Bus bus)
    {
        _db.Buses.Add(bus);
        await _db.SaveChangesAsync();
        return bus;
    }

    public async Task<Bus?> GetBusByIdAsync(Guid id)
    {
        return await _db.Buses.FindAsync(id);
    }

    public async Task<List<Bus>> GetBusesByOperatorAsync(Guid operatorId)
    {
        return await _db.Buses
            .Include(b => b.Route)
            .Include(b => b.BusStops)
            .Where(b => b.OperatorId == operatorId)
            .ToListAsync();
    }

    public async Task<BusStop> AddBusStopAsync(BusStop busStop)
    {
        _db.BusStops.Add(busStop);
        await _db.SaveChangesAsync();
        return busStop;
    }

    public async Task<BusStop?> GetBusStopByIdAsync(Guid stopId)
    {
        return await _db.BusStops.Include(s => s.Bus).FirstOrDefaultAsync(s => s.Id == stopId);
    }

    public async Task RemoveBusStopAsync(Guid stopId)
    {
        var stop = await _db.BusStops.FindAsync(stopId);
        if (stop is not null)
        {
            _db.BusStops.Remove(stop);
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateBusPriceAsync(Guid busId, decimal newPrice)
    {
        var bus = await _db.Buses.FindAsync(busId);
        if (bus is null) return;

        bus.BasePrice = newPrice;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateBusStaffAsync(Guid busId, string? dName, string? dPhone, string? cName, string? cPhone)
    {
        var bus = await _db.Buses.FindAsync(busId);
        if (bus is null) return;

        bus.DriverName     = dName;
        bus.DriverPhone    = dPhone;
        bus.ConductorName  = cName;
        bus.ConductorPhone = cPhone;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateBusStatusAsync(Guid busId, string status)
    {
        var bus = await _db.Buses.FindAsync(busId);
        if (bus is null) return;

        bus.Status = status;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Booking>> GetConfirmedFutureBookingsByBusAsync(Guid busId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _db.Bookings
            .Where(b => b.BusId == busId
                     && b.Status == "CONFIRMED"
                     && b.JourneyDate >= today)
            .ToListAsync();
    }

    public async Task CreateOperatingDaysAsync(List<BusOperatingDay> operatingDays)
    {
        _db.BusOperatingDays.AddRange(operatingDays);
        await _db.SaveChangesAsync();
    }

    public async Task<List<BusOperatingDay>> GetOperatingDaysByBusAsync(Guid busId)
    {
        return await _db.BusOperatingDays
            .Where(od => od.BusId == busId)
            .OrderBy(od => od.DayOfWeek)
            .ToListAsync();
    }

    public async Task UpdateOperatingDaysAsync(Guid busId, List<BusOperatingDay> operatingDays)
    {
        // Remove existing operating days for this bus
        var existingDays = await _db.BusOperatingDays
            .Where(od => od.BusId == busId)
            .ToListAsync();
        
        _db.BusOperatingDays.RemoveRange(existingDays);
        
        // Add new operating days
        _db.BusOperatingDays.AddRange(operatingDays);
        
        await _db.SaveChangesAsync();
    }
}
