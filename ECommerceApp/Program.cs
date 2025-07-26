using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy("AllowedFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
        });
    }
);

builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();
app.UseCors("AllowedFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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
    return Results.Created($"/api/orders/{orders.OrderID}",orders);
}).WithName("PostOrders");

app.Run();

public record OrderRequest(string Item,int Quantity);