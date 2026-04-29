using Busly.API.DTOs.Admin;
using Busly.API.DTOs.Auth;
using Busly.API.Models;

namespace Busly.API.Services;

public interface IAuthService
{
    Task<Customer> RegisterCustomerAsync(RegisterCustomerRequest request);
    Task<BusOperator> RegisterOperatorAsync(RegisterOperatorRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task AcceptTcAsync(Guid userId, string role);
    Task<TcStatusDto> GetTcStatusAsync(Guid userId, string role);
    Task<TcVersionDto?> GetCurrentTcAsync();
    Task<UserProfileResponse> GetProfileAsync(Guid userId, string role);
}
