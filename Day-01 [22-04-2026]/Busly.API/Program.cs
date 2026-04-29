using Busly.API.Data;
using Busly.API.Repositories;
using Busly.API.Services;
using Busly.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt; // Added for clearing claim map
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from the current directory or parent directory
var envFile = ".env";
var currentDir = Directory.GetCurrentDirectory();
var envPath = Path.Combine(currentDir, envFile);

// Check current dir, then parent dir
if (!File.Exists(envPath)) 
{
    envPath = Path.Combine(currentDir, "..", envFile);
}

if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath).Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")))
    {
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim().Trim('"').Trim('\'');
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

// Stop .NET from mapping "sub" to "http://schemas.xmlsoap..."
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// ── Connection string ─────────────────────────────────────────────────────────
// Prefer the environment variable; fall back to appsettings.json placeholder.
var connectionString =
    Environment.GetEnvironmentVariable("BUSLY_DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? string.Empty;

// ── EF Core / Npgsql ──────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── JWT configuration ─────────────────────────────────────────────────────────
var jwtSecret =
    Environment.GetEnvironmentVariable("BUSLY_JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"]
    ?? string.Empty;

var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "Busly";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Busly";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = false, // Relaxed for dev
            ValidateAudience         = false, // Relaxed for dev
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecret)),
            RoleClaimType            = ClaimTypes.Role,      // Ensure role is mapped correctly
            NameClaimType            = ClaimTypes.NameIdentifier // Use standard claim type
        };

        // Read JWT from the HttpOnly cookie instead of the Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("busly_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

// ── Role-based authorization policies ────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin",    policy => policy.RequireRole("Admin"));
    options.AddPolicy("Operator", policy => policy.RequireRole("Operator"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
});

// ── CORS — must allow credentials for HttpOnly cookie to be sent ──────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // required for cookies
});

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Busly API", Version = "v1" });

    // Allow JWT bearer tokens in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token (without the 'Bearer ' prefix)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IBusRepository, BusRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<ICancellationRepository, CancellationRepository>();
builder.Services.AddScoped<ITcRepository, TcRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<Busly.API.Services.IAuthService, Busly.API.Services.AuthService>();
builder.Services.AddScoped<Busly.API.Services.IAdminService, Busly.API.Services.AdminService>();
builder.Services.AddScoped<Busly.API.Services.IOperatorService, Busly.API.Services.OperatorService>();
builder.Services.AddScoped<Busly.API.Services.ISearchService, Busly.API.Services.SearchService>();
builder.Services.AddScoped<Busly.API.Services.ISeatLockService, Busly.API.Services.SeatLockService>();
builder.Services.AddScoped<Busly.API.Services.IBookingService, Busly.API.Services.BookingService>();
builder.Services.AddScoped<Busly.API.Services.IPdfService, Busly.API.Services.PdfService>();
builder.Services.AddSingleton<Busly.API.Services.IEmailService, Busly.API.Services.EmailService>();
builder.Services.AddScoped<Busly.API.Services.ICancellationService, Busly.API.Services.CancellationService>();
builder.Services.AddScoped<Busly.API.Services.IConfigService, Busly.API.Services.ConfigService>();
builder.Services.AddScoped<Busly.API.Services.IPnrService, Busly.API.Services.PnrService>();
builder.Services.AddScoped<Busly.API.Services.ICaptchaService, Busly.API.Services.CaptchaService>();

// ── Security Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<ISeatLockSecurityService, SeatLockSecurityService>();
builder.Services.AddScoped<IDateValidationService, DateValidationService>();
builder.Services.AddMemoryCache(); // For rate limiting
builder.Services.AddHttpContextAccessor(); // For IP address detection

// ── Background Jobs ───────────────────────────────────────────────────────────
builder.Services.AddHostedService<Busly.API.BackgroundJobs.PlatformCleanupJob>();
builder.Services.AddHostedService<Busly.API.BackgroundJobs.EmailDispatchJob>();
builder.Services.AddHostedService<SeatLockCleanupService>(); // Security cleanup service

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRateLimiting(); // Add rate limiting middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
