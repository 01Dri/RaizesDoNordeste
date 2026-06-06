using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RestauranteUni.API.Extensions;
using RestauranteUni.Application;
using RestauranteUni.Application.Services;
using RestauranteUni.Data;
using RestauranteUni.Domain.Services;
using Scalar.AspNetCore;
using System.Text;
using RestauranteUni.API.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IHasherService, HasherService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddApplicationServices(typeof(ApplicationAssemblyReference));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });



builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => 
    {
        options.WithTitle("RestauranteUni API");
        options.WithTheme(ScalarTheme.BluePlanet);
        options.WithDarkMode(true);
        options.WithHttpBearerAuthentication(new HttpBearerOptions());
    });

}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


// Source - https://stackoverflow.com/a/79785013
// Posted by Kevin Argueta, modified by community. See post 'Timeline' for change history
// Retrieved 2026-06-06, License - CC BY-SA 4.0

