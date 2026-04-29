using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Busly.API.Data;
using Busly.API.DTOs.Admin;
using Busly.API.DTOs.Auth;
using Busly.API.Models;
using Busly.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Busly.API.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public AuthService(IAuthRepository authRepo, IConfiguration config, AppDbContext db)
    {
        _authRepo = authRepo;
        _config   = config;
        _db       = db;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    public async Task<Customer> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        var customer = new Customer
        {
            Username     = request.Username,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            TcAccepted   = false,
            TcVersion    = null,
            TcAcceptedAt = null
        };

        return await _authRepo.CreateCustomerAsync(customer);
    }

    public async Task<BusOperator> RegisterOperatorAsync(RegisterOperatorRequest request)
    {
        var busOperator = new BusOperator
        {
            CompanyName  = request.CompanyName,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone        = request.Phone,
            Status       = "PENDING",
            TcAccepted   = false,
            TcVersion    = null,
            TcAcceptedAt = null
        };

        return await _authRepo.CreateOperatorAsync(busOperator);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        Guid   userId;
        string role;
        string email;

        // 1. Try Customer
        var customer = await _authRepo.GetCustomerByEmailAsync(request.Email);
        if (customer is not null && BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
        {
            userId = customer.Id;
            role   = "Customer";
            email  = customer.Email;
        }
        else
        {
            // 2. Try Operator
            var busOperator = await _authRepo.GetOperatorByEmailAsync(request.Email);
            if (busOperator is not null && BCrypt.Net.BCrypt.Verify(request.Password, busOperator.PasswordHash))
            {
                userId = busOperator.Id;
                role   = "Operator";
                email  = busOperator.Email;
            }
            else
            {
                // 3. Try Admin
                var admin = await _authRepo.GetAdminByEmailAsync(request.Email);
                if (admin is not null && BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
                {
                    userId = admin.Id;
                    role   = "Admin";
                    email  = admin.Email;
                }
                else
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }
            }
        }

        var token = GenerateJwt(userId, role, email);

        return new LoginResponse
        {
            Token  = token,
            Role   = role,
            Email  = email,
            UserId = userId
        };
    }

    // ── Accept T&C ────────────────────────────────────────────────────────────

    public async Task AcceptTcAsync(Guid userId, string role)
    {
        var activeTc = await _db.TcVersions
            .Where(t => t.IsActive)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No active T&C version found.");

        var now = DateTime.UtcNow;

        if (role == "Customer")
        {
            await _authRepo.UpdateCustomerTcAsync(userId, activeTc.Version, now);
        }
        else if (role == "Operator")
        {
            await _authRepo.UpdateOperatorTcAsync(userId, activeTc.Version, now);
        }
        else
        {
            throw new InvalidOperationException($"T&C acceptance is not supported for role '{role}'.");
        }
    }

    public async Task<TcStatusDto> GetTcStatusAsync(Guid userId, string role)
    {
        // Get current active T&C version
        var activeTc = await _db.TcVersions
            .Where(t => t.IsActive)
            .FirstOrDefaultAsync();

        string? lastAcceptedVersion = null;
        DateTime? lastAcceptedAt = null;
        bool hasAcceptedTc = false;

        // Get user's T&C acceptance status
        if (role == "Customer")
        {
            var customer = await _db.Customers
                .Where(c => c.Id == userId)
                .FirstOrDefaultAsync();
            
            if (customer != null)
            {
                hasAcceptedTc = customer.TcAccepted;
                lastAcceptedVersion = customer.TcVersion;
                lastAcceptedAt = customer.TcAcceptedAt;
            }
        }
        else if (role == "Operator")
        {
            var busOperator = await _db.BusOperators
                .Where(o => o.Id == userId)
                .FirstOrDefaultAsync();
            
            if (busOperator != null)
            {
                hasAcceptedTc = busOperator.TcAccepted;
                lastAcceptedVersion = busOperator.TcVersion;
                lastAcceptedAt = busOperator.TcAcceptedAt;
            }
        }

        return new TcStatusDto
        {
            HasAcceptedTc = hasAcceptedTc,
            LastAcceptedVersion = lastAcceptedVersion,
            LastAcceptedAt = lastAcceptedAt,
            CurrentActiveVersion = activeTc?.Version,
            NeedsToAcceptCurrent = hasAcceptedTc && activeTc != null && lastAcceptedVersion != activeTc.Version
        };
    }

    public async Task<TcVersionDto?> GetCurrentTcAsync()
    {
        var activeTc = await _db.TcVersions
            .Where(t => t.IsActive)
            .FirstOrDefaultAsync();

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

    // ── Get Profile ───────────────────────────────────────────────────────────

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, string role)
    {
        if (role == "Customer")
        {
            var customer = await _authRepo.GetCustomerByIdAsync(userId)
                ?? throw new KeyNotFoundException("Customer not found.");

            return new UserProfileResponse
            {
                UserId     = customer.Id,
                Email      = customer.Email,
                Role       = "Customer",
                Name       = customer.Name,
                TcAccepted = customer.TcAccepted,
                TcVersion  = customer.TcVersion
            };
        }
        else if (role == "Operator")
        {
            var busOperator = await _authRepo.GetOperatorByIdAsync(userId)
                ?? throw new KeyNotFoundException("Operator not found.");

            return new UserProfileResponse
            {
                UserId     = busOperator.Id,
                Email      = busOperator.Email,
                Role       = "Operator",
                Name       = busOperator.CompanyName,
                TcAccepted = busOperator.TcAccepted,
                TcVersion  = busOperator.TcVersion
            };
        }
        else if (role == "Admin")
        {
            var admin = await _db.Admins.FindAsync(userId)
                ?? throw new KeyNotFoundException("Admin not found.");

            return new UserProfileResponse
            {
                UserId     = admin.Id,
                Email      = admin.Email,
                Role       = "Admin",
                Name       = admin.Username,
                TcAccepted = true,
                TcVersion  = null
            };
        }
        else
        {
            throw new InvalidOperationException($"Unknown role '{role}'.");
        }
    }

    // ── JWT Helper ────────────────────────────────────────────────────────────

    private string GenerateJwt(Guid userId, string role, string email)
    {
        var secret = Environment.GetEnvironmentVariable("BUSLY_JWT_SECRET")
                     ?? _config["Jwt:Secret"]
                     ?? string.Empty;

        var issuer   = _config["Jwt:Issuer"]   ?? "Busly";
        var audience = _config["Jwt:Audience"] ?? "Busly";
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var mins) ? mins : 60;

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new Claim(ClaimTypes.Role,               role),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
