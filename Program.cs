using System.Globalization;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Serilog;
using SFManagement.Api.Configuration;
using SFManagement.Api.Middleware;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Mappings;
using SFManagement.Application.Validators;
using SFManagement.Application.Validators.Transactions;
using SFManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var enableDetailedLogging = builder.Configuration.GetValue<bool>("EnableDetailedLogging");
var runMigrationsOnStartup = builder.Configuration.GetValue<bool>("RunMigrationsOnStartup");
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting SF Management API");

    builder.AddStandardServices();
    builder.AddScopedServices();
    builder.AddAuthServices();
    builder.AddHealthCheckServices();
    builder.Services.AddResponseCaching();
    builder.Services.AddMemoryCache();
    builder.AddRateLimitServices();
    builder.Services.AddApplicationInsightsTelemetry();

    builder.Services.AddDbContext<DataContext>(p =>
    {
        p.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            o => o
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                .EnableRetryOnFailure(6, TimeSpan.FromSeconds(15), null)
            )
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());

        if (builder.Environment.IsDevelopment() || enableDetailedLogging)
        {
            p.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
        }
    });

    // Remove Identity configuration - Auth0 handles authentication
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<WalletTransactionValidator>();
    builder.Services.AddScoped<IValidator<UpdateFiatAssetTransactionRequest>, UpdateFiatAssetTransactionValidator>();
    builder.Services.AddScoped<IValidator<UpdateDigitalAssetTransactionRequest>, UpdateDigitalAssetTransactionValidator>();

    builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        options.DefaultRequestCulture = new RequestCulture("pt-BR");
        options.SupportedCultures = new List<CultureInfo> { new("pt-BR") };
        options.RequestCultureProviders.Clear();
    });

    // It configures JSON serialization at the application level
    // This is used to avoid circular references in the JSON response
    // It is used to avoid the error:
    // "A possible object cycle was detected which is not supported. This can either be due to a cycle or if the object depth is too large."
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

    var app = builder.Build();

    app.UseCors(x =>
    {
        if (allowedOrigins.Length > 0)
        {
            x.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            return;
        }

        Log.Warning("No CORS origins configured. Falling back to AllowAnyOrigin without credentials.");
        x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    
    app.UseIpRateLimiting();
    app.UseResponseCaching();
    app.UseHttpsRedirection();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseStaticFiles();

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger(opts =>
        {
            //opts.SerializeAsV2 = true;
        });
        // Custom themes: https://github.com/ostranme/swagger-ui-themes
        app.UseSwaggerUI(opts =>
        {
            opts.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            opts.RoutePrefix = string.Empty;
            opts.InjectStylesheet("/css/theme-modern.css");
        });
    }

    app.UseAuthentication();
    
    // Add authentication logging middleware AFTER authentication
    app.UseAuthenticationLogging();
    
    // Add detailed request/response logging middleware (especially for debugging 400 errors)
    // Only enable in development or when debugging issues
    if (app.Environment.IsDevelopment() || enableDetailedLogging)
    {
        app.UseRequestResponseLogging();
    }
    
    app.UseAuthorization();
    app.UseMiddleware<ErrorHandlerMiddleware>();
    app.MapControllers();
    app.MapHealthChecks("/health").AllowAnonymous();

    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    var cultureInfo = new CultureInfo("pt-BR");
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    CultureInfo.CurrentCulture = cultureInfo;
    CultureInfo.CurrentUICulture = cultureInfo;

    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture(cultureInfo),
        SupportedCultures = new List<CultureInfo> { cultureInfo },
        SupportedUICultures = new List<CultureInfo> { cultureInfo }
    });

    if (runMigrationsOnStartup)
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        serviceScope.ServiceProvider.GetRequiredService<DataContext>().Database.Migrate();
    }

    // Remove Identity seeding - Auth0 handles user management
    // using (var scope = app.Services.CreateScope())
    // {
    //     var services = scope.ServiceProvider;
    //     try
    //     {
    //         var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    //         var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    //         await ApplicationDbContextSeed.SeedEssentialsAsync(userManager, roleManager);
    //     }
    //     catch (Exception ex)
    //     {
    //     }
    // }

    Log.Information("SF Management API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SF Management API failed to start");
}
finally
{
    Log.CloseAndFlush();
}