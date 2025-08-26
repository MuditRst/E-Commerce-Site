using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Confluent.Kafka;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Identity.Data;

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
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("Jwt"))
                {
                    context.Token = context.Request.Cookies["Jwt"];
                }
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

builder.Services.AddAuthorization();

var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);
builder.Services.AddHostedService<KafkaConsumerService>();

var adminClient = new AdminClientBuilder(new AdminClientConfig
{
    BootstrapServers = "localhost:9092"
}).Build();

var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
Console.WriteLine($"Kafka topics available: {string.Join(", ", metadata.Topics.Select(t => t.Topic))}");


var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowedFrontend");

//auth here
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<OrderHub>("/hubs/orders");

// Orders CRUD endpoints

app.MapGet("/api/orders", async (DBContext db) =>
{
    var orders = await db.Orders.Select(
        o => new
        {
            o.OrderID,
            o.Item,
            o.Quantity,
            OrderStatus = o.OrderStatus.ToString(),
            o.UserID,
            User = o.User != null ? o.User.Username : null
        }
    ).ToListAsync();
    return Results.Ok(orders);
}).WithName("GetOrders");

app.MapPost("/api/orders", async (DBContext db,OrderRequest res,ClaimsPrincipal claims) =>
{
    var userIdclaim = claims.FindFirst(ClaimTypes.NameIdentifier);
    if(userIdclaim == null)
        return Results.Unauthorized();
    _ = int.TryParse(userIdclaim?.Value, out int userId);
    var orders = new Orders { Item = res.Item, Quantity = res.Quantity , UserID = userId,OrderStatus = 0 };
    db.Orders.Add(orders);
    await db.SaveChangesAsync();

    var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
    using var producer = new ProducerBuilder<Null, string>(config).Build();


    var message = JsonSerializer.Serialize(orders, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    await producer.ProduceAsync("order-topic", new Message<Null, string> { Value = message });
    
    Console.WriteLine($"Produced order to Kafka: {message}");

    return Results.Created($"/api/orders/{orders.OrderID}", orders);
}).RequireAuthorization().WithName("PostOrders");

app.MapPut("/api/orders/{id}", async (int id, DBContext db, [FromBody] Orders updatedOrder, ClaimsPrincipal claims) =>
{
    var userIdclaim = claims.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdclaim == null)
        return Results.Unauthorized();
    _ = int.TryParse(userIdclaim?.Value, out int userId);
    var orders = await db.Orders.FindAsync(id);

    if (orders == null) return Results.NotFound();
    if (orders.UserID == userId)
    {
        orders.Item = updatedOrder.Item;
        orders.Quantity = updatedOrder.Quantity;
    }
    else
    {
        return Results.Unauthorized();
    }

    await db.SaveChangesAsync();
    return Results.Ok(orders);

}).RequireAuthorization();

app.MapPut("/api/orders/{id}/status", async (int id, DBContext db, [FromBody] OrderStatus newStatus, ClaimsPrincipal user) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order == null) return Results.NotFound();

    db.OrderStatusHistories.Add(new OrderStatusHistory
    {
        OrderID = order.OrderID,
        OrderStatus = newStatus,
        ChangedBy = user.Identity?.Name,
        ChangedAt = DateTime.UtcNow
    });

    order.OrderStatus = newStatus;

    await db.SaveChangesAsync();
    return Results.Ok(order);
});

app.MapDelete("/api/orders", async (DBContext db, [FromBody] Orders order, ClaimsPrincipal claims) =>
{
    var userIdclaim = claims.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdclaim == null)
        return Results.Unauthorized();
    _ = int.TryParse(userIdclaim?.Value, out int userId);
    var orders = await db.Orders.FirstOrDefaultAsync(o => o.Item == order.Item && o.Quantity == order.Quantity);

    if (orders == null) return Results.NotFound();
    if (orders.UserID == userId)
        db.Orders.Remove(orders);
    else
        return Results.Unauthorized();
    await db.SaveChangesAsync();
    return Results.Ok(orders);
}).RequireAuthorization();

// Kafka|SignalR endpoints

app.MapGet("/api/kafka/logs", async(DBContext db,ClaimsPrincipal claims)=>
{
    var userRole = claims.FindFirst(ClaimTypes.Role)?.Value;

    if (userRole != "Admin")
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    var kafkalogs = await db.KafkaLogs.ToListAsync();

    if (kafkalogs == null)
    {
        return Results.NotFound("No Kafka logs found");
    }

    return Results.Ok(kafkalogs);
});

// User Authentication and Authorization endpoints

app.MapGet("/api/user/me", (ClaimsPrincipal user) =>
{
    var username = user.Identity?.Name;
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new { username, userId, role = userRole });
}).RequireAuthorization();

app.MapPost("/api/auth/register", async (DBContext db, [FromBody] LoginDatabase userToRegister) =>
{
    var user = await db.Logins.AnyAsync(u => u.Username == userToRegister.Username);
    if (user)
    {
        return Results.Conflict("User already exists");
    }

    userToRegister.Password = PasswordHelper.HashPassword(userToRegister.Password);
    db.Logins.Add(userToRegister);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapPost("/api/auth/login", async (DBContext db, HttpContext http, [FromBody] LoginDatabase user) =>
{
    http.Response.Cookies.Append("Jwt", "", new CookieOptions
    {
        Expires = DateTimeOffset.UtcNow.AddDays(-1),
        HttpOnly = true,
        Secure = !app.Environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict
    });

    var dbUser = await db.Logins.FirstOrDefaultAsync(u => u.Username == user.Username);
    if (dbUser == null || !PasswordHelper.VerifyPassword(dbUser.Password, user.Password))
    {
        http.Response.Cookies.Append("Jwt", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure = !app.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict
        });
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, dbUser.UserID.ToString()),
        new Claim(ClaimTypes.Name,dbUser.Username),
        new Claim(ClaimTypes.Role,dbUser.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]!)),
        signingCredentials: creds);

    string jwt = new JwtSecurityTokenHandler().WriteToken(token);

    http.Response.Cookies.Append("Jwt", jwt, new CookieOptions
    {
        HttpOnly = true,
        Secure = !app.Environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"] ?? "60"))
    });

    return Results.Ok(new { Token = jwt ,dbUser.Username, dbUser.UserID, dbUser.Role});
});

app.MapPut("/api/auth/forgetpassword", async (DBContext db, [FromBody] LoginDatabase ResetUser) =>
{
    var user = await db.Logins.FirstOrDefaultAsync(u => u.Username == ResetUser.Username);

    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    //TODO: Token based reset password for security

    user.Password = PasswordHelper.HashPassword(ResetUser.Password);

    await db.SaveChangesAsync();
    return Results.Ok("Password Changed Successfully");
});

app.MapGet("/api/users", async (DBContext db) =>
{
    var users = await db.Logins.Select(u =>
    new
    {
        u.UserID,
        u.Username,
        u.Role

    }).ToListAsync();
    if (users == null)
    {
        return Results.NotFound("No users Found");
    }

    return Results.Ok(users);
});

app.MapDelete("/api/users/{id}", async (int id,DBContext db, ClaimsPrincipal claims) =>
{
    var userRole = claims.FindFirst(ClaimTypes.Role)?.Value.ToString();

    if (userRole != "Admin")
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    var user = await db.Logins.FindAsync(id);

    if (user == null)
    {
        return Results.NotFound("User Not Found");
    }

    db.Logins.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok(new {Message = "User deleted successfully"});
});

app.MapPost("/api/auth/logout", (HttpContext http) =>
{
    http.Response.Cookies.Delete("Jwt");
    return Results.Ok(new { message = "Logged out successfully" });
});


//charts endpoint

app.MapGet("/api/orders/stats", async(DBContext db)=>
{
    var stats = await db.Orders.GroupBy(o => o.OrderStatus).Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync();

    return Results.Ok(stats);
}).RequireAuthorization();

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