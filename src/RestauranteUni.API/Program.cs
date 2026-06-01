using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestauranteUni.Application.UseCases.Accounts;
using RestauranteUni.Application.UseCases.Accounts.Validations;
using RestauranteUni.Application.Utils;
using RestauranteUni.Data;
using RestauranteUni.Domain.Accounts.DTO;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IHasher, Hasher>();
builder.Services.AddScoped<IValidator<CreateAccountDto>, CreateAccountDtoValidation>();
builder.Services.AddScoped<
        IUseCaseHandler<CreateAccountDto, CreateAccountResponseDto>,
        CreateAccountUseCaseHandler>();


// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
