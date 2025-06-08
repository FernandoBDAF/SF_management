using System.Globalization;
using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SFManagement;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.StartupConfig;
using SFManagement.ViewModels.Validators;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.AddStandardServices();
builder.AddScopedServices();
builder.AddAuthServices();
builder.AddHealthCheckServices();
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();
builder.AddRateLimitServices();

builder.Services.AddDbContext<DataContext>(p =>
    p.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<DataContext>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddFluentValidation(config =>
{
    config.RegisterValidatorsFromAssemblyContaining<WalletTransactionValidator>();
});


builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = new List<CultureInfo> { new("pt-BR") };
    options.RequestCultureProviders.Clear();
});

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseIpRateLimiting();
app.UseResponseCaching();
app.UseHttpsRedirection();
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

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    serviceScope.ServiceProvider.GetService<DataContext>().Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await ApplicationDbContextSeed.SeedEssentialsAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
    }
}

app.Run();