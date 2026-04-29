using Busly.API.DTOs.Admin;
using Busly.API.Repositories;
using BuslyRoute = Busly.API.Models.Route;
using BusOperator = Busly.API.Models.BusOperator;
using Bus = Busly.API.Models.Bus;
using TcVersion = Busly.API.Models.TcVersion;

namespace Busly.API.Services;

public class AdminService : IAdminService
{
    private readonly IRouteRepository _routeRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly IAuthRepository _authRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly ITcRepository _tcRepo;

    public AdminService(
        IRouteRepository routeRepo,
        IAuditRepository auditRepo,
        IAuthRepository authRepo,
        IBookingRepository bookingRepo,
        ITcRepository tcRepo)
    {
        _routeRepo   = routeRepo;
        _auditRepo   = auditRepo;
        _authRepo    = authRepo;
        _bookingRepo = bookingRepo;
        _tcRepo      = tcRepo;
    }

    // ── Routes ────────────────────────────────────────────────────────────────

    public async Task<RouteDto> CreateRouteAsync(CreateRouteRequest request, Guid adminId)
    {
        var existingRoute = await _routeRepo.GetByCitiesAsync(request.SourceCity, request.DestinationCity);
        
        if (existingRoute != null)
        {
            if (existingRoute.IsActive)
            {
                throw new InvalidOperationException("Route already exists and is active.");
            }

            // Re-activate the existing inactive route
            existingRoute.IsActive = true;
            await _routeRepo.UpdateAsync(existingRoute);
            await _auditRepo.LogAsync(adminId, "admin", "REACTIVATE_ROUTE", "route", existingRoute.Id);
            // Return 200-style DTO with a flag so the caller knows it was a reactivation, not a new create
            return MapRoute(existingRoute);
        }

        var route = new BuslyRoute
        {
            Id             = Guid.NewGuid(),
            SourceCity     = request.SourceCity,
            DestinationCity = request.DestinationCity,
            IsActive       = true,
            CreatedByAdmin = adminId
        };

        await _routeRepo.CreateAsync(route);
        await _auditRepo.LogAsync(adminId, "admin", "CREATE_ROUTE", "route", route.Id, 
            $"{{\"source\":\"{route.SourceCity}\", \"destination\":\"{route.DestinationCity}\"}}");

        return MapRoute(route);
    }

    public async Task<List<RouteDto>> GetAllRoutesAsync()
    {
        var routes = await _routeRepo.GetAllAsync();
        return routes.Select(MapRoute).ToList();
    }

    public async Task<List<RouteDto>> GetActiveRoutesAsync()
    {
        var routes = await _routeRepo.GetAllActiveAsync();
        return routes.Select(MapRoute).ToList();
    }

    public async Task ToggleRouteAsync(Guid routeId, Guid adminId)
    {
        await _routeRepo.ToggleAsync(routeId);
        await _auditRepo.LogAsync(adminId, "admin", "TOGGLE_ROUTE", "route", routeId);
    }

    // ── Operators ─────────────────────────────────────────────────────────────

    public async Task<List<OperatorDto>> GetPendingOperatorsAsync()
    {
        var operators = await _authRepo.GetPendingOperatorsAsync();
        return operators.Select(MapOperator).ToList();
    }

    public async Task<List<OperatorDto>> GetAllOperatorsAsync()
    {
        var operators = await _authRepo.GetAllOperatorsAsync();
        return operators.Select(MapOperator).ToList();
    }

    public async Task ApproveOperatorAsync(Guid operatorId, Guid adminId)
    {
        await _authRepo.UpdateOperatorStatusAsync(operatorId, "APPROVED", adminId);
        await _auditRepo.LogAsync(adminId, "admin", "APPROVE_OPERATOR", "bus_operator", operatorId, "{\"status\":\"APPROVED\"}");
    }

    public async Task RejectOperatorAsync(Guid operatorId, Guid adminId)
    {
        await _authRepo.UpdateOperatorStatusAsync(operatorId, "REJECTED");
        await _auditRepo.LogAsync(adminId, "admin", "REJECT_OPERATOR", "bus_operator", operatorId);
    }

    public async Task ToggleOperatorAsync(Guid operatorId, Guid adminId)
    {
        var op = await _authRepo.GetOperatorByIdAsync(operatorId)
            ?? throw new InvalidOperationException("Operator not found");

        var newStatus = op.Status == "APPROVED" ? "DISABLED" : "APPROVED";

        await _authRepo.UpdateOperatorStatusAsync(operatorId, newStatus);
        await _auditRepo.LogAsync(adminId, "admin", "TOGGLE_OPERATOR", "bus_operator", operatorId);
    }

    // ── Buses ─────────────────────────────────────────────────────────────────

    public async Task<List<BusDto>> GetPendingBusesAsync()
    {
        var buses = await _authRepo.GetPendingBusesAsync();
        return buses.Select(MapBus).ToList();
    }

    public async Task<List<BusDto>> GetAllBusesAsync()
    {
        var buses = await _authRepo.GetAllBusesAsync();
        return buses.Select(MapBus).ToList();
    }

    public async Task<List<BusDto>> GetBusesByOperatorAsync(Guid operatorId)
    {
        var buses = await _authRepo.GetBusesByOperatorAsync(operatorId);
        return buses.Select(MapBus).ToList();
    }

    public async Task ApproveBusAsync(Guid busId, Guid adminId)
    {
        await _authRepo.UpdateBusStatusAsync(busId, "ACTIVE", adminId);
        await _auditRepo.LogAsync(adminId, "admin", "APPROVE_BUS", "bus", busId, "{\"status\":\"ACTIVE\"}");
    }

    public async Task RejectBusAsync(Guid busId, Guid adminId)
    {
        await _authRepo.UpdateBusStatusAsync(busId, "REJECTED", adminId);
        await _auditRepo.LogAsync(adminId, "admin", "REJECT_BUS", "bus", busId);
    }

    public async Task ToggleBusStatusAsync(Guid busId, Guid adminId)
    {
        var bus = await _authRepo.GetBusByIdAsync(busId);
        if (bus == null) throw new InvalidOperationException("Bus not found");

        if (bus.Status == "PENDING")
            throw new InvalidOperationException("Cannot toggle a pending bus. Use Approve/Reject.");

        var newStatus = bus.Status == "ACTIVE" ? "DISABLED" : "ACTIVE";
        await _authRepo.UpdateBusStatusAsync(busId, newStatus, adminId);
        await _auditRepo.LogAsync(adminId, "admin", "TOGGLE_BUS_STATUS", "bus", busId, $"{{\"status\":\"{newStatus}\"}}");
    }

    // ── Revenue ───────────────────────────────────────────────────────────────

    public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync()
    {
        return await _bookingRepo.GetMonthlyRevenueAsync();
    }

    public async Task<List<OperatorRevenueDto>> GetOperatorRevenueAsync()
    {
        return await _bookingRepo.GetOperatorRevenueAsync();
    }

    // ── T&C Management ────────────────────────────────────────────────────────

    public async Task PublishTcVersionAsync(CreateTcRequest request, Guid adminId)
    {
        var tc = new TcVersion
        {
            Id               = Guid.NewGuid(),
            Version          = request.Version,
            Content          = request.Content,
            PublishedAt      = DateTime.UtcNow,
            EffectiveAt      = request.EffectiveAt ?? DateTime.UtcNow,
            PublishedByAdmin = adminId,
            IsActive         = true
        };

        await _tcRepo.CreateAsync(tc);
        await _tcRepo.DeactivateAllExceptAsync(tc.Id);
        
        await _auditRepo.LogAsync(adminId, "admin", "PUBLISH_TC", "tc_version", tc.Id, $"{{\"version\":\"{tc.Version}\"}}");
    }

    public async Task<List<TcVersionDto>> GetAllTcVersionsAsync()
    {
        var versions = await _tcRepo.GetAllAsync();
        return versions.Select(v => new TcVersionDto
        {
            Id          = v.Id,
            Version     = v.Version,
            Content     = v.Content,
            PublishedAt = v.PublishedAt,
            EffectiveAt = v.EffectiveAt,
            IsActive    = v.IsActive
        }).ToList();
    }

    public async Task<TcVersionDto?> GetCurrentTcAsync()
    {
        var activeTc = await _tcRepo.GetActiveAsync();
        
        if (activeTc == null)
            return null;

        return new TcVersionDto
        {
            Id = activeTc.Id,
            Version = activeTc.Version,
            Content = activeTc.Content,
            PublishedAt = activeTc.PublishedAt,
            EffectiveAt = activeTc.EffectiveAt,
            IsActive = activeTc.IsActive
        };
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync()
    {
        var logs = await _auditRepo.GetAllAsync();
        return logs.Select(MapAuditLog).ToList();
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static RouteDto MapRoute(BuslyRoute r) => new()
    {
        Id              = r.Id,
        SourceCity      = r.SourceCity,
        DestinationCity = r.DestinationCity,
        IsActive        = r.IsActive
    };

    private static OperatorDto MapOperator(BusOperator op) => new()
    {
        Id          = op.Id,
        CompanyName = op.CompanyName,
        Email       = op.Email,
        Phone       = op.Phone,
        ContactEmail = op.Email,
        ContactPhone = op.Phone,
        Status      = op.Status,
        CreatedAt   = op.CreatedAt,
        TcVersion   = op.TcVersion,
        TcAcceptedAt = op.TcAcceptedAt
    };

    private static BusDto MapBus(Bus b) => new()
    {
        Id              = b.Id,
        BusNumber       = b.BusNumber,
        BusName         = b.BusName,
        OwnerName       = b.OwnerName,
        OwnerPhone      = b.OwnerPhone,
        OwnerEmail      = b.OwnerEmail,
        BasePrice       = b.BasePrice,
        DriverName      = b.DriverName,
        DriverPhone     = b.DriverPhone,
        ConductorName   = b.ConductorName,
        ConductorPhone  = b.ConductorPhone,
        SourceCity      = b.Route?.SourceCity,
        DestinationCity = b.Route?.DestinationCity,
        LayoutName      = b.Layout?.LayoutName,
        Status          = b.Status,
        OperatorId      = b.OperatorId,
        RouteId         = b.RouteId,
        CreatedAt       = b.CreatedAt,
        OperatingDays   = b.OperatingDays?.Select(od => new OperatingDayDto 
        { 
            DayOfWeek = od.DayOfWeek, 
            IsActive = od.IsActive 
        }).ToList() ?? new List<OperatingDayDto>()
    };

    private static AuditLogDto MapAuditLog(Busly.API.Models.AuditLog log) => new()
    {
        Id         = log.Id,
        ActorId    = log.ActorId,
        ActorRole  = log.ActorRole ?? "unknown",
        Action     = log.Action,
        EntityType = log.EntityType,
        EntityId   = log.EntityId,
        Metadata   = log.Metadata,
        Timestamp  = log.PerformedAt
    };
}
