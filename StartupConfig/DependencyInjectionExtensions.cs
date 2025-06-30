using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.Settings;

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
            Description = "JWT Authorization header info using bearer tokens",
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
        builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());

        builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidAudience = builder.Configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    builder.Configuration["JWT:Key"]
                ))
            };
        });
    }

    public static void AddScopedServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<BaseService<Address>, AddressService>();
        builder.Services.AddScoped<AddressService>();
        builder.Services.AddScoped<BaseService<Bank>, BankService>();
        builder.Services.AddScoped<BankService>();
        builder.Services.AddScoped<BaseAssetHolderService<Client>, ClientService>();
        builder.Services.AddScoped<ClientService>();
        builder.Services.AddScoped<BaseService<ContactPhone>, ContactPhoneService>();
        builder.Services.AddScoped<ContactPhoneService>();
        builder.Services.AddScoped<BaseService<InitialBalance>, InitialBalanceService>();
        builder.Services.AddScoped<InitialBalanceService>();
        builder.Services.AddScoped<BaseService<Member>, MemberService>();
        builder.Services.AddScoped<MemberService>();
        builder.Services.AddScoped<BaseService<PokerManager>, PokerManagerService>();
        builder.Services.AddScoped<PokerManagerService>();
        builder.Services.AddScoped<BaseService<AssetWallet>, AssetWalletService>();
        builder.Services.AddScoped<AssetWalletService>();
        builder.Services.AddScoped<BaseService<WalletIdentifier>, WalletIdentifierService>();
        builder.Services.AddScoped<WalletIdentifierService>();
        
        builder.Services.AddScoped<BaseTransactionService<FiatAssetTransaction>, FiatAssetTransactionService>();
        builder.Services.AddScoped<FiatAssetTransactionService>();
        builder.Services.AddScoped<OfxService>();
        builder.Services.AddScoped<BaseService<Ofx>, OfxService>();
        // builder.Services.AddScoped<TransactionService>();
        builder.Services.AddScoped<BaseTransactionService<DigitalAssetTransaction>, DigitalAssetTransactionService>();
        builder.Services.AddScoped<DigitalAssetTransactionService>();
        builder.Services.AddScoped<BaseService<Excel>, ExcelService>();
        builder.Services.AddScoped<ExcelService>();
        builder.Services.AddScoped<BaseService<Tag>, TagService>();
        builder.Services.AddScoped<TagService>();
        // builder.Services.AddScoped<BaseService<InternalTransaction>, InternalTransactionService>();
        // builder.Services.AddScoped<InternalTransactionService>();
        builder.Services.AddScoped<UserResolverService>();
        builder.Services.AddScoped<BaseService<AvgRate>, AvgRateService>();
        builder.Services.AddScoped<AvgRateService>();
        builder.Services.AddScoped<UserService>();
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