using Microsoft.EntityFrameworkCore;
using Stock_Server.Data;
using Stock_Server.Repository;
using Stock_Server.Models;
using Stock_Server.Service;
using Stock_Server.Mapper;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

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
                var key = keyValue[0];
                var value = keyValue[1];
                if (key == "sslmode")
                    connectionString += $"SSL Mode={value};";
                else if (key == "channel_binding")
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
builder.Services.AddScoped<SupplierRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<JSONProductMapper>();
builder.Services.AddScoped<JSONSupplierMapper>();
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

app.MapControllers();

app.Run();
