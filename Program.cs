using Microsoft.EntityFrameworkCore;
using Stock_Server.Data;
using Stock_Server.Repository;
using Stock_Server.Models;
using Stock_Server.Service;
using Stock_Server.Mapper;
using DotNetEnv;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
Env.Load();

var builder = WebApplication.CreateBuilder(args);
// ---------------------------
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? builder.Configuration["JwtSettings:SecretKey"];
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
             ?? builder.Configuration["JwtSettings:Issuer"];
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
               ?? builder.Configuration["JwtSettings:Audience"];

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException("JWT_SECRET must be at least 32 characters");

var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = !string.IsNullOrEmpty(issuer),
            ValidIssuer = issuer,
            ValidateAudience = !string.IsNullOrEmpty(audience),
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero   // optional, strict expiry
        };
        
        // 🔥 CRITICAL: Read token from cookie "auth"
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["auth"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = Environment.GetEnvironmentVariable("DB_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is not configured. Set the DB_URL environment variable or add DefaultConnection in appsettings.json.");
}

// If the connection string is in URI format, convert it to Npgsql format
if (connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo[1];
    var host = uri.Host;
    var port = uri.Port == -1 ? 5432 : uri.Port; // Default PostgreSQL port
    var database = uri.AbsolutePath.TrimStart('/');
    var queryString = uri.Query.TrimStart('?');
    
    connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database};";
    
    // Parse query parameters manually
    if (!string.IsNullOrEmpty(queryString))
    {
        var parameters = queryString.Split('&');
        foreach (var param in parameters)
        {
            var keyValue = param.Split('=');
            if (keyValue.Length == 2)
            {
                var key_1 = keyValue[0];
                var value = keyValue[1];
                if (key_1 == "sslmode")
                    connectionString += $"SSL Mode={value};";
                else if (key_1 == "channel_binding")
                    connectionString += $"Channel Binding={value};";
            }
        }
    }
}

builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IRepository<Supplier>, SupplierRepository>();
builder.Services.AddScoped<IUserRepo, UserRepository>();
builder.Services.AddScoped<IRepository<User>, UserRepository>(); 

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JSONProductMapper>();
builder.Services.AddScoped<JSONSupplierMapper>();
builder.Services.AddScoped<JSONUserMapper>();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StockDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
 
app.UseAuthentication();   // validates token, sets User
app.UseAuthorization(); 
app.MapControllers();

app.Run();
