using Accounting.Api;
using Accounting.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<AccountStore>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("Fintech"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAccountEndpoints();

app.MapTransactionEndpoints();

app.UseHttpsRedirection();

app.Run();