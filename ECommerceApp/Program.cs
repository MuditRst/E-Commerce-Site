using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Confluent.Kafka;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5097");
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy("AllowedFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        });
    }
);

builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);
builder.Services.AddHostedService<KafkaConsumerService>();

var adminClient = new AdminClientBuilder(new AdminClientConfig
{
    BootstrapServers = "localhost:9092"
}).Build();

var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
Console.WriteLine($"Kafka topics available: {string.Join(", ", metadata.Topics.Select(t => t.Topic))}");


var app = builder.Build();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowedFrontend");

//auth here

app.MapHub<OrderHub>("/hubs/orders");
app.UseHttpsRedirection();

app.MapGet("/api/orders", async (DBContext db) =>
{
    var orders = await db.Orders.ToListAsync();
    return Results.Ok(orders);
}).WithName("GetOrders");

app.MapPost("/api/orders", async (DBContext db,OrderRequest res) =>
{
    var orders = new Orders { Item = res.Item, Quantity = res.Quantity };
    db.Orders.Add(orders);
    await db.SaveChangesAsync();

    var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
    using var producer = new ProducerBuilder<Null, string>(config).Build();


    var message = JsonSerializer.Serialize(orders);
    await producer.ProduceAsync("order-topic", new Message<Null, string> { Value = message });
    
    Console.WriteLine($"Produced order to Kafka: {message}");

    return Results.Created($"/api/orders/{orders.OrderID}", orders);
}).WithName("PostOrders");

app.MapPut("/api/orders", async (DBContext db, Orders updatedOrder) =>
{
    var orders = await db.Orders.FirstOrDefaultAsync(o => o.Item == updatedOrder.Item);

    if (orders == null) return Results.NotFound();

    orders.Quantity = updatedOrder.Quantity;
    await db.SaveChangesAsync();
    return Results.Ok(orders);

});

app.MapDelete("/api/orders", async (DBContext db, [FromBody] Orders order) =>
{
    var orders = await db.Orders.FirstOrDefaultAsync(o => o.Item == order.Item && o.Quantity == order.Quantity);

    if (orders == null) return Results.NotFound();

    db.Orders.Remove(orders);
    await db.SaveChangesAsync();
    return Results.Ok(orders);
});

app.MapGet("/", () => "Backend is running ✅");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"🚨 App failed to start: {ex.Message}");
}


public record OrderRequest(string Item,int Quantity);