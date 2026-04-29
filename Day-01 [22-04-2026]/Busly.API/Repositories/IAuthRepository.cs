using Busly.API.Models;

namespace Busly.API.Repositories;

public interface IAuthRepository
{
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<BusOperator?> GetOperatorByEmailAsync(string email);
    Task<Admin?> GetAdminByEmailAsync(string email);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<BusOperator> CreateOperatorAsync(BusOperator busOperator);
    Task<Customer?> GetCustomerByIdAsync(Guid id);
    Task<BusOperator?> GetOperatorByIdAsync(Guid id);
    Task UpdateCustomerTcAsync(Guid customerId, string tcVersion, DateTime acceptedAt);
    Task UpdateOperatorTcAsync(Guid operatorId, string tcVersion, DateTime acceptedAt);

    // Operator status management
    Task UpdateOperatorStatusAsync(Guid operatorId, string status, Guid? approvedByAdmin = null);
    Task<List<BusOperator>> GetPendingOperatorsAsync();
    Task<List<BusOperator>> GetAllOperatorsAsync();

    // Bus management
    Task<Bus?> GetBusByIdAsync(Guid id);
    Task UpdateBusStatusAsync(Guid busId, string status, Guid? approvedByAdmin = null);
    Task<List<Bus>> GetPendingBusesAsync();
    Task<List<Bus>> GetAllBusesAsync();
    Task<List<Bus>> GetBusesByOperatorAsync(Guid operatorId);
}
