using Microsoft.EntityFrameworkCore;
using Stock_Server.Models;

namespace Stock_Server.Data;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products{get;set;}
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<User> Users { get; set; }
}
