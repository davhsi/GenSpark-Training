using Busly.API.Data;
using Busly.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _db;

    public AuthRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Customer?> GetCustomerByEmailAsync(string email) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Email == email);

    public Task<BusOperator?> GetOperatorByEmailAsync(string email) =>
        _db.BusOperators.FirstOrDefaultAsync(o => o.Email == email);

    public Task<Admin?> GetAdminByEmailAsync(string email) =>
        _db.Admins.FirstOrDefaultAsync(a => a.Email == email);

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task<BusOperator> CreateOperatorAsync(BusOperator busOperator)
    {
        _db.BusOperators.Add(busOperator);
        await _db.SaveChangesAsync();
        return busOperator;
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id) =>
        await _db.Customers.FindAsync(id);

    public async Task<BusOperator?> GetOperatorByIdAsync(Guid id) =>
        await _db.BusOperators.FindAsync(id);

    public async Task UpdateCustomerTcAsync(Guid customerId, string tcVersion, DateTime acceptedAt)
    {
        var customer = await _db.Customers.FindAsync(customerId);
        if (customer is null) return;

        customer.TcVersion = tcVersion;
        customer.TcAcceptedAt = acceptedAt;
        customer.TcAccepted = true;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateOperatorTcAsync(Guid operatorId, string tcVersion, DateTime acceptedAt)
    {
        var busOperator = await _db.BusOperators.FindAsync(operatorId);
        if (busOperator is null) return;

        busOperator.TcVersion = tcVersion;
        busOperator.TcAcceptedAt = acceptedAt;
        busOperator.TcAccepted = true;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateOperatorStatusAsync(Guid operatorId, string status, Guid? approvedByAdmin = null)
    {
        var busOperator = await _db.BusOperators.FindAsync(operatorId);
        if (busOperator is null) return;

        busOperator.Status = status;

        if (approvedByAdmin.HasValue)
        {
            busOperator.ApprovedByAdmin = approvedByAdmin;
            busOperator.ApprovedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public Task<List<BusOperator>> GetPendingOperatorsAsync() =>
        _db.BusOperators.Where(o => o.Status == "PENDING").ToListAsync();

    public Task<List<BusOperator>> GetAllOperatorsAsync() =>
        _db.BusOperators
            .OrderByDescending(op => op.CreatedAt)
            .ToListAsync();

    public async Task<Bus?> GetBusByIdAsync(Guid id) =>
        await _db.Buses.FindAsync(id);

    public async Task UpdateBusStatusAsync(Guid busId, string status, Guid? approvedByAdmin = null)
    {
        var bus = await _db.Buses.FindAsync(busId);
        if (bus is null) return;

        bus.Status = status;

        if (approvedByAdmin.HasValue)
        {
            bus.ApprovedByAdmin = approvedByAdmin;
            bus.ApprovedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public Task<List<Bus>> GetPendingBusesAsync() =>
        _db.Buses
            .Include(b => b.Route)
            .Include(b => b.Layout)
            .Include(b => b.OperatingDays)
            .Where(b => b.Status == "PENDING")
            .ToListAsync();

    public Task<List<Bus>> GetAllBusesAsync() =>
        _db.Buses
            .Include(b => b.Route)
            .Include(b => b.Layout)
            .Include(b => b.Operator)
            .Include(b => b.OperatingDays)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public Task<List<Bus>> GetBusesByOperatorAsync(Guid operatorId) =>
        _db.Buses
            .Include(b => b.Route)
            .Include(b => b.Layout)
            .Include(b => b.OperatingDays)
            .Where(b => b.OperatorId == operatorId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
}
