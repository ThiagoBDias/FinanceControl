using FinanceControl.Application.Interfaces;
using FinanceControl.Infrastructure.Auth;
using FinanceControl.Infrastructure.Persistence;
using FinanceControl.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FinanceControl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceControlInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration,
        string sessionCookie)
    {
        services.AddDbContext<FinanceControlDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<FinanceControlDbContext>()
        .AddDefaultTokenProviders();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var environmentSecret = Environment.GetEnvironmentVariable("FINANCECONTROL_JWT_SECRET");
        if (!string.IsNullOrWhiteSpace(environmentSecret))
            jwt.Secret = environmentSecret;
        if (string.IsNullOrWhiteSpace(jwt.Secret) || Encoding.UTF8.GetByteCount(jwt.Secret) < 32)
            throw new InvalidOperationException("JWT Secret ausente ou curto. Configure Jwt:Secret ou FINANCECONTROL_JWT_SECRET.");

        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience) ||
            jwt.ExpirationMinutes is < 5 or > 120)
            throw new InvalidOperationException("Configure emissor, público e validade do JWT entre 5 e 120 minutos.");

        services.Configure<JwtOptions>(options =>
        {
            options.Secret = jwt.Secret;
            options.Issuer = jwt.Issuer;
            options.Audience = jwt.Audience;
            options.ExpirationMinutes = jwt.ExpirationMinutes;
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (!context.Request.Headers.ContainsKey("Authorization") &&
                            context.Request.Cookies.TryGetValue(sessionCookie, out var token))
                            context.Token = token;
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IDebtRepository, DebtRepository>();

        return services;
    }
}
