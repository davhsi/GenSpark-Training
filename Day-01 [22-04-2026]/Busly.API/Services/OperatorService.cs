using System.Text.Json;
using Busly.API.DTOs.Operator;
using Busly.API.Models;
using Busly.API.Repositories;

namespace Busly.API.Services;

public class OperatorService : IOperatorService
{
    private readonly IBusRepository _busRepo;
    private readonly ISeatRepository _seatRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ICancellationService _cancellationService;
    private readonly IAuthRepository _authRepo;

    public OperatorService(
        IBusRepository busRepo,
        ISeatRepository seatRepo,
        IAuditRepository auditRepo,
        ICancellationService cancellationService,
        IAuthRepository authRepo)
    {
        _busRepo              = busRepo;
        _seatRepo             = seatRepo;
        _auditRepo            = auditRepo;
        _cancellationService  = cancellationService;
        _authRepo             = authRepo;
    }

    private async Task EnsureOperatorApprovedAsync(Guid operatorId)
    {
        var op = await _authRepo.GetOperatorByIdAsync(operatorId);
        if (op is null || op.Status != "APPROVED")
        {
            throw new UnauthorizedAccessException("Operator account is not approved.");
        }
    }

    public async Task<OperatorProfileDto> GetProfileAsync(Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        return await GetProfileWithoutApprovalCheckAsync(operatorId);
    }

    public async Task<OperatorProfileDto> GetProfileWithoutApprovalCheckAsync(Guid operatorId)
    {
        var op = await _authRepo.GetOperatorByIdAsync(operatorId);
        if (op is null)
            throw new UnauthorizedAccessException("Operator not found.");

        return new OperatorProfileDto
        {
            Id          = op.Id.ToString(),
            CompanyName = op.CompanyName,
            Email       = op.Email,
            Phone       = op.Phone ?? string.Empty,
            Status      = op.Status ?? string.Empty,
            ApprovedAt  = op.ApprovedAt,
            CreatedAt   = op.CreatedAt
        };
    }

    // ── Layout ────────────────────────────────────────────────────────────────

    public async Task<LayoutDto> CreateLayoutAsync(CreateLayoutRequest request, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var jsonString = JsonSerializer.Serialize(request.SeatConfig);
        var seatCount  = request.SeatConfig.Seats.Count;

        var layout = new BusLayout
        {
            Id         = Guid.NewGuid(),
            OperatorId = operatorId,
            LayoutName = request.LayoutName,
            SeatConfig = jsonString,
            TotalSeats = seatCount
        };

        await _busRepo.CreateLayoutAsync(layout);

        var seats = request.SeatConfig.Seats.Select(item => new Seat
        {
            Id         = Guid.NewGuid(),
            LayoutId   = layout.Id,
            SeatNumber = item.SeatNumber,
            SeatType   = item.Type,
            Deck       = item.Deck,
            Row        = item.Row,
            Col        = item.Col
        }).ToList();

        await _seatRepo.BulkInsertSeatsAsync(seats);
        await _auditRepo.LogAsync(operatorId, "operator", "CREATE_LAYOUT", "bus_layout", layout.Id, $"{{\"name\":\"{layout.LayoutName}\", \"seats\":{layout.TotalSeats}}}");

        return new LayoutDto
        {
            Id         = layout.Id,
            LayoutName = layout.LayoutName,
            TotalSeats = layout.TotalSeats
        };
    }

    public async Task<List<LayoutDto>> GetLayoutsAsync(Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var layouts = await _busRepo.GetLayoutsByOperatorAsync(operatorId);

        return layouts.Select(l => new LayoutDto
        {
            Id         = l.Id,
            LayoutName = l.LayoutName,
            TotalSeats = l.TotalSeats,
            SeatConfig = new SeatConfigDto
            {
                Rows  = l.Seats.Any() ? l.Seats.Max(s => s.Row) : 0,
                Cols  = l.Seats.Any() ? l.Seats.Max(s => s.Col) : 0,
                Decks = l.Seats.Select(s => s.Deck ?? "lower").Distinct().ToList(),
                Seats = l.Seats.Select(s => new SeatItemDto
                {
                    SeatNumber = s.SeatNumber ?? 0,
                    Row        = s.Row,
                    Col        = s.Col,
                    Type       = s.SeatType ?? "standard",
                    Deck       = s.Deck ?? "lower"
                }).ToList()
            }
        }).ToList();
    }

    public async Task RemoveLayoutAsync(Guid layoutId, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        
        // Verify the layout belongs to the operator
        var layout = await _busRepo.GetLayoutByIdAsync(layoutId);
        if (layout == null || layout.OperatorId != operatorId)
        {
            throw new UnauthorizedAccessException("Layout not found or access denied.");
        }
        
        var inUse = await _busRepo.IsLayoutInUseAsync(layoutId);
        if (inUse)
            throw new InvalidOperationException("Cannot delete layout. It is currently assigned to one or more buses.");

        await _busRepo.RemoveLayoutAsync(layoutId);
        await _auditRepo.LogAsync(operatorId, "operator", "REMOVE_LAYOUT", "bus_layout", layoutId);
    }

    // ── Bus ───────────────────────────────────────────────────────────────────

    public async Task<BusDetailDto> RegisterBusAsync(RegisterBusRequest request, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var bus = new Bus
        {
            Id         = Guid.NewGuid(),
            OperatorId = operatorId,
            RouteId    = request.RouteId,
            LayoutId   = request.LayoutId,
            BusNumber  = request.BusNumber,
            BusName        = request.BusName,
            OwnerName      = request.OwnerName,
            OwnerPhone     = request.OwnerPhone,
            OwnerEmail     = request.OwnerEmail,
            DriverName     = request.DriverName,
            DriverPhone    = request.DriverPhone,
            ConductorName  = request.ConductorName,
            ConductorPhone = request.ConductorPhone,
            BasePrice      = request.BasePrice,
            Status         = "PENDING",
            CreatedAt      = DateTime.UtcNow
        };

        await _busRepo.CreateBusAsync(bus);
        
        // Create operating days if provided
        if (request.OperatingDays != null && request.OperatingDays.Any())
        {
            var operatingDays = request.OperatingDays.Select(od => new BusOperatingDay
            {
                Id = Guid.NewGuid(),
                BusId = bus.Id,
                DayOfWeek = od.DayOfWeek,
                IsActive = od.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
            
            await _busRepo.CreateOperatingDaysAsync(operatingDays);
        }
        else
        {
            // Default to all days active
            var defaultOperatingDays = Enumerable.Range(1, 7).Select(day => new BusOperatingDay
            {
                Id = Guid.NewGuid(),
                BusId = bus.Id,
                DayOfWeek = day,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
            
            await _busRepo.CreateOperatingDaysAsync(defaultOperatingDays);
        }
        
        await _auditRepo.LogAsync(operatorId, "operator", "REGISTER_BUS", "bus", bus.Id, $"{{\"number\":\"{bus.BusNumber}\", \"name\":\"{bus.BusName}\"}}");

        return new BusDetailDto
        {
            Id        = bus.Id,
            BusNumber = bus.BusNumber,
            BusName        = bus.BusName,
            OwnerName      = bus.OwnerName,
            OwnerPhone     = bus.OwnerPhone,
            OwnerEmail     = bus.OwnerEmail,
            DriverName     = bus.DriverName,
            DriverPhone    = bus.DriverPhone,
            ConductorName  = bus.ConductorName,
            ConductorPhone = bus.ConductorPhone,
            Status         = bus.Status,
            BasePrice      = bus.BasePrice,
            RouteId        = bus.RouteId,
            LayoutId       = bus.LayoutId,
            CreatedAt      = bus.CreatedAt,
            OperatingDays = request.OperatingDays ?? Enumerable.Range(1, 7).Select(day => new OperatingDayDto { DayOfWeek = day, IsActive = true }).ToList()
        };
    }

    public async Task<List<BusDetailDto>> GetBusesAsync(Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var buses = await _busRepo.GetBusesByOperatorAsync(operatorId);

        var result = new List<BusDetailDto>();
        
        foreach (var bus in buses)
        {
            var operatingDays = await _busRepo.GetOperatingDaysByBusAsync(bus.Id);
            
            result.Add(new BusDetailDto
            {
                Id              = bus.Id,
                BusNumber       = bus.BusNumber,
                BusName         = bus.BusName,
                OwnerName       = bus.OwnerName,
                OwnerPhone      = bus.OwnerPhone,
                OwnerEmail      = bus.OwnerEmail,
                DriverName      = bus.DriverName,
                DriverPhone     = bus.DriverPhone,
                ConductorName   = bus.ConductorName,
                ConductorPhone  = bus.ConductorPhone,
                Status          = bus.Status,
                BasePrice       = bus.BasePrice,
                SourceCity      = bus.Route?.SourceCity,
                DestinationCity = bus.Route?.DestinationCity,
                RouteId         = bus.RouteId,
                LayoutId        = bus.LayoutId,
                CreatedAt       = bus.CreatedAt,
                OperatingDays   = operatingDays.Select(od => new OperatingDayDto 
                { 
                    DayOfWeek = od.DayOfWeek, 
                    IsActive = od.IsActive 
                }).ToList(),
                Stops = bus.BusStops.Select(s => new BusStopDto
                {
                    Id = s.Id,
                    Type = s.Type,
                    City = s.City,
                    Address = s.Address,
                    ScheduledTime = s.ScheduledTime?.ToString("HH:mm") ?? ""
                }).ToList()
            });
        }
        
        return result;
    }

    // ── Bus stops ─────────────────────────────────────────────────────────────

    public async Task AddBusStopAsync(Guid busId, AddBusStopRequest request, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var busStop = new BusStop
        {
            Id            = Guid.NewGuid(),
            BusId         = busId,
            Type          = request.Type,
            City          = request.City,
            Address       = request.Address,
            ScheduledTime = TimeOnly.Parse(request.ScheduledTime)
        };

        await _busRepo.AddBusStopAsync(busStop);
    }

    public async Task RemoveBusStopAsync(Guid stopId, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var stop = await _busRepo.GetBusStopByIdAsync(stopId);
        if (stop is null || stop.Bus?.OperatorId != operatorId)
            throw new UnauthorizedAccessException("You do not own this stop.");

        await _busRepo.RemoveBusStopAsync(stopId);
        await _auditRepo.LogAsync(operatorId, "operator", "REMOVE_STOP", "bus_stop", stopId);
    }

    // ── Price ─────────────────────────────────────────────────────────────────

    public async Task UpdateBusPriceAsync(Guid busId, UpdatePriceRequest request, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var bus = await _busRepo.GetBusByIdAsync(busId);
        if (bus is null || bus.OperatorId != operatorId)
            throw new UnauthorizedAccessException("Bus not found or access denied.");
        await _busRepo.UpdateBusPriceAsync(busId, request.BasePrice);
        await _auditRepo.LogAsync(operatorId, "operator", "UPDATE_PRICE", "bus", busId, $"{{\"basePrice\":{request.BasePrice}}}");
    }

    public async Task UpdateBusStaffAsync(Guid busId, UpdateStaffRequest request, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        var bus = await _busRepo.GetBusByIdAsync(busId);
        if (bus is null || bus.OperatorId != operatorId)
            throw new UnauthorizedAccessException("Bus not found or access denied.");
        await _busRepo.UpdateBusStaffAsync(busId, request.DriverName, request.DriverPhone, request.ConductorName, request.ConductorPhone);
        await _auditRepo.LogAsync(operatorId, "operator", "UPDATE_STAFF", "bus", busId);
    }

    public async Task UpdateOperatingDaysAsync(UpdateOperatingDaysRequest request, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        
        // Verify the bus belongs to the operator
        var bus = await _busRepo.GetBusByIdAsync(request.BusId);
        if (bus == null || bus.OperatorId != operatorId)
        {
            throw new UnauthorizedAccessException("Bus not found or access denied.");
        }

        var operatingDays = request.OperatingDays.Select(od => new BusOperatingDay
        {
            Id = Guid.NewGuid(),
            BusId = request.BusId,
            DayOfWeek = od.DayOfWeek,
            IsActive = od.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await _busRepo.UpdateOperatingDaysAsync(request.BusId, operatingDays);
        await _auditRepo.LogAsync(operatorId, "operator", "UPDATE_OPERATING_DAYS", "bus", request.BusId);
    }

    // ── Status ────────────────────────────────────────────────────────────────

    public async Task DisableBusAsync(Guid busId, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        // Run cascade FIRST (inside its own transaction), then update bus status.
        // This ensures if cascade fails, the bus is not left DISABLED with un-cancelled bookings.
        await _cancellationService.ProcessOperatorCascadeAsync(busId, operatorId);
        await _busRepo.UpdateBusStatusAsync(busId, "DISABLED");
        await _auditRepo.LogAsync(operatorId, "operator", "DISABLE_BUS", "bus", busId);
    }

    public async Task RemoveBusAsync(Guid busId, Guid operatorId)
    {
        await EnsureOperatorApprovedAsync(operatorId);
        // Run cascade FIRST, then update bus status.
        await _cancellationService.ProcessOperatorCascadeAsync(busId, operatorId);
        await _busRepo.UpdateBusStatusAsync(busId, "REMOVED");
        await _auditRepo.LogAsync(operatorId, "operator", "REMOVE_BUS", "bus", busId);
    }
}
