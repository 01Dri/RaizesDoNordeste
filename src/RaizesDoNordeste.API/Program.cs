using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaizesDoNordeste.API;
using RaizesDoNordeste.API.Extensions;
using RaizesDoNordeste.API.OpenApi;
using RaizesDoNordeste.Application;
using RaizesDoNordeste.Application.Services;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUserContext>();

builder.Services.AddScoped<IHasherService, HasherService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ILoyalityProgramService, LoyalityProgramService>();
builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<UninterPayment.SDK.IUninterPaymentClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var workerUrl = config["WorkerUrl"] ?? config["WORKER_URL"] ?? "http://localhost:5200";
    var httpClientFactory = sp.GetService<IHttpClientFactory>();
    var httpClient = httpClientFactory?.CreateClient() ?? new HttpClient();
    return new UninterPayment.SDK.UninterPaymentClient(httpClient, workerUrl);
});

builder.Services.AddApplicationServices(typeof(ApplicationAssemblyReference));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});
builder.Services.AddPatterns();

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.MapOpenApi();
app.MapScalarApiReference(options => 
{
    options.WithTitle("RaizesDoNordeste API");
    options.WithTheme(ScalarTheme.BluePlanet);
    options.WithDarkMode(true);
    options.WithHttpBearerAuthentication(new HttpBearerOptions());
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
