using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UninterPayment.Worker;

var builder = WebApplication.CreateBuilder(args);

// Register payment queue as singleton
builder.Services.AddSingleton<PaymentQueue>();

// Register background worker
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapPost("/payments", async ([FromBody] PaymentQueueItem item, [FromServices] PaymentQueue queue) =>
{
    if (item == null || string.IsNullOrWhiteSpace(item.TransactionId))
    {
        return Results.BadRequest("Invalid payment details.");
    }
    
    await queue.EnqueueAsync(item);
    return Results.Accepted();
});

app.MapGet("/health", () => Results.Ok("Worker is running."));

app.Run("http://localhost:5200");
