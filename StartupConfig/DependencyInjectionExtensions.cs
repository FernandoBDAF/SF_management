using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using SFManagement.Models;
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
        var securityScheme = new OpenApiSecurityScheme()
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
                },
            });
        });
    }

    private static void AddVersioningServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(opts =>
        {
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.DefaultApiVersion = new(1, 0);
            opts.ReportApiVersions = true;
        });

        builder.Services.AddVersionedApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
            opts.SubstituteApiVersionInUrl = true;
        });
    }
    // public static void AddCustomServices(this WebApplicationBuilder builder)
    // {
    //     builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
    //     builder.Services.AddSingleton<ITodoData, TodoData>();
    // }
    
    public static void AddAuthServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        
        builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
            };
        });
    }

    public static void AddScopedServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ClientService>();
        builder.Services.AddScoped<BaseService<Client>, ClientService>();
        builder.Services.AddScoped<BankService>();
        builder.Services.AddScoped<BaseService<Bank>, BankService>();
        builder.Services.AddScoped<BankTransactionService>();
        builder.Services.AddScoped<BaseService<BankTransaction>, BankTransactionService>();
        builder.Services.AddScoped<OfxService>();
        builder.Services.AddScoped<BaseService<Ofx>, OfxService>();
        builder.Services.AddScoped<TransactionService>();
        builder.Services.AddScoped<BaseService<Manager>, ManagerService>();
        builder.Services.AddScoped<ManagerService>();
        builder.Services.AddScoped<BaseService<Wallet>, WalletService>();
        builder.Services.AddScoped<WalletService>();
        builder.Services.AddScoped<BaseService<Nickname>, NicknameService>();
        builder.Services.AddScoped<NicknameService>();
        builder.Services.AddScoped<BaseService<WalletTransaction>, WalletTransactionService>();
        builder.Services.AddScoped<WalletTransactionService>();
        builder.Services.AddScoped<BaseService<Excel>, ExcelService>();
        builder.Services.AddScoped<ExcelService>();
        builder.Services.AddScoped<BaseService<Tag>, TagService>();
        builder.Services.AddScoped<TagService>();
        builder.Services.AddScoped<BaseService<ClosingWallet>, ClosingWalletService>();
        builder.Services.AddScoped<ClosingWalletService>();
        builder.Services.AddScoped<BaseService<ClosingNickname>, ClosingNicknameService>();
        builder.Services.AddScoped<ClosingNicknameService>();
        builder.Services.AddScoped<BaseService<ClosingManager>, ClosingManagerService>();
        builder.Services.AddScoped<ClosingManagerService>();
        builder.Services.AddScoped<BaseService<InternalTransaction>, InternalTransactionService>();
        builder.Services.AddScoped<InternalTransactionService>();
        builder.Services.AddScoped<UserResolverService>();
        builder.Services.AddScoped<BaseService<AvgRate>, AvgRateService>();
        builder.Services.AddScoped<AvgRateService>();
    }
    
    // public static void AddHealthCheckServices(this WebApplicationBuilder builder)
    // {
    //     builder.Services.AddHealthChecks()
    //         .AddSqlServer(builder.Configuration.GetConnectionString("Default"));
    // }
}