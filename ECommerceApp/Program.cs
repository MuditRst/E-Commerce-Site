using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

//Kestrel Setup

builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    options.ListenAnyIP(int.Parse(port));
});

// CosmosDB Setup
string? cosmosConnStr = Environment.GetEnvironmentVariable("COSMOSDB_CONN_STRING");

if (string.IsNullOrEmpty(cosmosConnStr))
{
    throw new InvalidOperationException("COSMOSDB_CONN_STRING is missing in environment variables.");
}

builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseCosmos(
        cosmosConnStr,
        databaseName: "ordersdb"
    );
});

// CORS
var allowedOrigins = "https://orange-flower-079d22910.2.azurestaticapps.net";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// JSON options
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// JWT Authentication
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
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if(!app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowedFrontend");

//auth here
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<OrderHub>("/hubs/orders");

// Orders CRUD endpoints
app.MapGet("/api/orders", async (DBContext db) =>
{
    var orders = await db.Orders.ToListAsync();
    var uniqueUserIds = orders.Select(o => o.UserId).Distinct().ToList();
    var users = await db.Logins.Where(u => uniqueUserIds.Contains(u.ID)).ToListAsync();

    var ordersWithUser = orders.Select(o => new
    {
        o.ID,
        o.Item,
        o.Quantity,
        o.OrderStatus,
        o.UserId,
        User = users.FirstOrDefault(u => u.ID == o.UserId)?.Username
    });

    return Results.Ok(ordersWithUser);
}).WithName("GetOrders");;

app.MapPost("/api/orders", async (DBContext db, OrderRequest res, ClaimsPrincipal claims, KafkaProducerService kafkaProducer) =>
{
    var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
        return Results.Unauthorized();

    var userId = userIdClaim.Value;
    var order = new Orders { Item = res.Item, Quantity = res.Quantity, UserId = userId, OrderStatus = 0 };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    await kafkaProducer.ProduceAsync(order, order.ID);

    return Results.Created($"/api/orders/{order.ID}", order);
}).RequireAuthorization().WithName("PostOrders");

app.MapPut("/api/orders/{id}", async (string id, DBContext db, [FromBody] Orders updatedOrder, ClaimsPrincipal claims) =>
{
    var userIdclaim = claims.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdclaim == null)
        return Results.Unauthorized();
    var userId = userIdclaim.Value;
    var orders = await db.Orders.FirstOrDefaultAsync(o=> o.ID == id && o.UserId == userId);

    if (orders == null) return Results.NotFound();
    if (orders.UserId == userId)
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

app.MapPut("/api/orders/{id}/status", async (string id, DBContext db, [FromBody] OrderStatus newStatus, ClaimsPrincipal user) =>
{
    var order = await db.Orders.FindAsync(id,user.FindFirstValue(ClaimTypes.NameIdentifier));
    if (order == null) return Results.NotFound();

    db.OrderStatusHistories.Add(new OrderStatusHistory
    {
        OrderID = order.ID,
        UserId = order.UserId,
        OrderStatus = newStatus,
        ChangedBy = user.Identity?.Name,
        ChangedAt = DateTime.UtcNow
    });

    order.OrderStatus = newStatus;

    await db.SaveChangesAsync();
    return Results.Ok(order);
});

app.MapDelete("/api/orders/{id}", async (string id,DBContext db, ClaimsPrincipal claims) =>
{
    var userIdclaim = claims.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdclaim == null)
        return Results.Unauthorized();
    var userId = userIdclaim.Value;
    var orders = await db.Orders.FindAsync(id,userId);

    if (orders == null) return Results.NotFound();
    if (orders.UserId == userId)
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
    var user = await db.Logins.FindAsync(userToRegister.Username,userToRegister.Username);
    if (user != null)
    {
        return Results.Conflict("User already exists");
    }

    userToRegister.Password = PasswordHelper.HashPassword(userToRegister.Password);
    userToRegister.ID = userToRegister.Username;
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
        Secure = true,
        SameSite = SameSiteMode.None
    });

    var dbUser = await db.Logins.FirstOrDefaultAsync(u => u.Username == user.Username);
    if (dbUser == null || !PasswordHelper.VerifyPassword(dbUser.Password, user.Password))
    {
        http.Response.Cookies.Append("Jwt", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, dbUser.ID.ToString()),
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
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = DateTimeOffset.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"] ?? "60"))
    });

    return Results.Ok(new { Token = jwt ,dbUser.Username, dbUser.ID, dbUser.Role});
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
        u.ID,
        u.Username,
        u.Role

    }).ToListAsync();
    if (users == null)
    {
        return Results.NotFound("No users Found");
    }

    return Results.Ok(users);
});

app.MapDelete("/api/users/{id}", async (string id,DBContext db, ClaimsPrincipal claims) =>
{
    var userRole = claims.FindFirst(ClaimTypes.Role)?.Value.ToString();

    if (userRole != "Admin")
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    var user = await db.Logins.FindAsync(id,id);

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

//Health Checkups

app.MapGet("/", () => "Backend is running ✅");

app.MapGet("/health", async (DBContext db) =>
{
    try
    {
        await db.Orders.FirstOrDefaultAsync();
        return Results.Ok(new { status = "Healthy ✅" });
    }
    catch
    {
        return Results.StatusCode(500);
    }
});

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"🚨 App failed to start: {ex.Message}");
    throw;
}

public record OrderRequest(string Item,int Quantity);