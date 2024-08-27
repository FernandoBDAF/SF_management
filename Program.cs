using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using SFManagement;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.Settings;
using SFManagement.ViewModels;
using SFManagement.ViewModels.Validators;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

builder.Services.AddDbContext<DataContext>(p => p.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<DataContext>();
builder.Services.AddScoped<UserService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = false;
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

builder.Services.AddScoped<IValidator<ClientRequest>, ClientRequestValidator>();
builder.Services.AddScoped<IValidator<BankRequest>, BankRequestValidator>();
builder.Services.AddScoped<IValidator<BankTransactionRequest>, BankTransactionRequestValidator>();
builder.Services.AddScoped<IValidator<OfxRequest>, OfxRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<TokenRequest>, TokenRequestValidator>();
builder.Services.AddScoped<IValidator<AddRoleRequest>, AddRoleRequestValidator>();
builder.Services.AddScoped<IValidator<ManagerRequest>, ManagerValidator>();
builder.Services.AddScoped<IValidator<WalletRequest>, WalletValidator>();
builder.Services.AddScoped<IValidator<NicknameRequest>, NicknameValidator>();
builder.Services.AddScoped<IValidator<WalletTransactionRequest>, WalletTransactionValidator>();
builder.Services.AddScoped<IValidator<ImportBuySellTransactionsRequest>, ImportBuyTransactionsRequestValidator>();
builder.Services.AddScoped<IValidator<ExcelRequest>, ExcelRequestValidator>();

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
builder.Services.AddScoped<ExcelService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    serviceScope.ServiceProvider.GetService<DataContext>().Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();

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
