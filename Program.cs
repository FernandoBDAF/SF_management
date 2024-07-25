using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SFManagement;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;
using SFManagement.ViewModels.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(p => p.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddScoped<IValidator<ClientRequest>, ClientRequestValidator>();
builder.Services.AddScoped<IValidator<BankRequest>, BankRequestValidator>();
builder.Services.AddScoped<IValidator<BankTransactionRequest>, BankTransactionRequestValidator>();
builder.Services.AddScoped<IValidator<OfxRequest>, OfxRequestValidator>();

builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<BaseService<Client>, ClientService>();
builder.Services.AddScoped<BankService>();
builder.Services.AddScoped<BaseService<Bank>, BankService>();
builder.Services.AddScoped<BankTransactionService>();
builder.Services.AddScoped<BaseService<BankTransaction>, BankTransactionService>();
builder.Services.AddScoped<OfxService>();
builder.Services.AddScoped<BaseService<Ofx>, OfxService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();


app.Run();
