using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SFManagement.Interfaces;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Entities;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.Settings;
using SFManagement.Authorization;

namespace SFManagement.StartupConfig;

public static class DependencyInjectionExtensions
{
    public static void AddStandardServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.AddSwaggerServices();
        builder.AddVersioningServices();
    }

    private static void AddSwaggerServices(this WebApplicationBuilder builder)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Auth0 JWT Authorization header info using bearer tokens",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        var securityRequirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "bearerAuth"
                    }
                },
                new string[] { }
            }
        };

        builder.Services.AddSwaggerGen(opts =>
        {
            opts.AddSecurityDefinition("bearerAuth", securityScheme);
            opts.AddSecurityRequirement(securityRequirement);
            opts.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Sempre Fichas API",
                Description = "This is the API for the Sempre Fichas Management System.",
                Contact = new OpenApiContact
                {
                    Name = "Fernando Barroso",
                    Url = new Uri("https://github.com/FernandoBDAF")
                }
            });
        });
    }

    private static void AddVersioningServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(opts =>
        {
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.DefaultApiVersion = new ApiVersion(1, 0);
            opts.ReportApiVersions = true;
        });

        builder.Services.AddVersionedApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
            opts.SubstituteApiVersionInUrl = true;
        });
    }

    public static void AddAuthServices(this WebApplicationBuilder builder)
    {
        // Configure Auth0 settings
        builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection("Auth0"));

        // Add Auth0 user service
        builder.Services.AddScoped<IAuth0UserService, Auth0UserService>();

        // Configure authorization policies
        builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("Role:admin", policy => policy.Requirements.Add(new RoleRequirement(Auth0Roles.Admin)))
            .AddPolicy("Role:manager", policy => policy.Requirements.Add(new RoleRequirement(Auth0Roles.Manager)))
            .AddPolicy("Role:user", policy => policy.Requirements.Add(new RoleRequirement(Auth0Roles.User)))
            .AddPolicy("Role:viewer", policy => policy.Requirements.Add(new RoleRequirement(Auth0Roles.Viewer)))
            .AddPolicy("Permission:read:users", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.ReadUsers)))
            .AddPolicy("Permission:create:users", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.CreateUsers)))
            .AddPolicy("Permission:update:users", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.UpdateUsers)))
            .AddPolicy("Permission:delete:users", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.DeleteUsers)))
            .AddPolicy("Permission:read:clients", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.ReadClients)))
            .AddPolicy("Permission:create:clients", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.CreateClients)))
            .AddPolicy("Permission:update:clients", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.UpdateClients)))
            .AddPolicy("Permission:delete:clients", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.DeleteClients)))
            .AddPolicy("Permission:read:transactions", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.ReadTransactions)))
            .AddPolicy("Permission:create:transactions", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.CreateTransactions)))
            .AddPolicy("Permission:update:transactions", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.UpdateTransactions)))
            .AddPolicy("Permission:delete:transactions", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.DeleteTransactions)))
            .AddPolicy("Permission:read:financial_data", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.ReadFinancialData)))
            .AddPolicy("Permission:create:financial_data", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.CreateFinancialData)))
            .AddPolicy("Permission:update:financial_data", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.UpdateFinancialData)))
            .AddPolicy("Permission:delete:financial_data", policy => policy.Requirements.Add(new PermissionRequirement(Auth0Permissions.DeleteFinancialData)));

        // Add authorization handlers
        builder.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Configure JWT Bearer authentication for Auth0
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
            o.Audience = builder.Configuration["Auth0:Audience"];
            o.RequireHttpsMetadata = false;
            o.SaveToken = true;
        });
    }

    public static void AddScopedServices(this WebApplicationBuilder builder)
    {
        // Domain services
        builder.Services.AddScoped<IAssetHolderDomainService, AssetHolderDomainService>();
        
        // Entity services
        builder.Services.AddScoped<BaseService<Address>, AddressService>();
        builder.Services.AddScoped<AddressService>();
        builder.Services.AddScoped<BaseAssetHolderService<Bank>, BankService>();
        builder.Services.AddScoped<BankService>();
        builder.Services.AddScoped<BaseAssetHolderService<Client>, ClientService>();
        builder.Services.AddScoped<ClientService>();
        builder.Services.AddScoped<BaseService<ContactPhone>, ContactPhoneService>();
        builder.Services.AddScoped<ContactPhoneService>();
        builder.Services.AddScoped<BaseService<InitialBalance>, InitialBalanceService>();
        builder.Services.AddScoped<InitialBalanceService>();
        builder.Services.AddScoped<BaseAssetHolderService<Member>, MemberService>();
        builder.Services.AddScoped<MemberService>();
        builder.Services.AddScoped<BaseAssetHolderService<PokerManager>, PokerManagerService>();
        builder.Services.AddScoped<PokerManagerService>();
        builder.Services.AddScoped<BaseService<AssetPool>, AssetPoolService>();
        builder.Services.AddScoped<AssetPoolService>();
        builder.Services.AddScoped<AssetPoolValidationService>();
        builder.Services.AddScoped<BaseService<WalletIdentifier>, WalletIdentifierService>();
        builder.Services.AddScoped<WalletIdentifierService>();
        
        // Transaction services
        builder.Services.AddScoped<BaseTransactionService<FiatAssetTransaction>, FiatAssetTransactionService>();
        builder.Services.AddScoped<FiatAssetTransactionService>();
        builder.Services.AddScoped<OfxService>();
        builder.Services.AddScoped<BaseService<Ofx>, OfxService>();
        // builder.Services.AddScoped<TransactionService>();
        builder.Services.AddScoped<BaseTransactionService<DigitalAssetTransaction>, DigitalAssetTransactionService>();
        builder.Services.AddScoped<DigitalAssetTransactionService>();
        builder.Services.AddScoped<BaseService<Excel>, ExcelService>();
        builder.Services.AddScoped<ExcelService>();
        builder.Services.AddScoped<BaseTransactionService<SettlementTransaction>, SettlementTransactionService>();
        builder.Services.AddScoped<SettlementTransactionService>();
        
        // Other services
        builder.Services.AddScoped<BaseService<Category>, CategoryService>();
        builder.Services.AddScoped<CategoryService>();
        // builder.Services.AddScoped<BaseService<InternalTransaction>, InternalTransactionService>();
        // builder.Services.AddScoped<InternalTransactionService>();
        // Note: UserService and UserResolverService will be removed as they're replaced by Auth0
        
        // Add logging service
        builder.Services.AddScoped<ILoggingService, LoggingService>();
    }

    public static void AddHealthCheckServices(this WebApplicationBuilder builder)
    {
        // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        builder.Services.AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }

    public static void AddRateLimitServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<IpRateLimitOptions>(
            builder.Configuration.GetSection("IpRateLimiting"));
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        builder.Services.AddInMemoryRateLimiting();
    }
}