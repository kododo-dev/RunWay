using Kododo.RunWay.Demo.WebApp.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.Demo.WebApp.Database;

public class DemoDbContext : DbContext
{
    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }
    
    public DbSet<DbProduct>  Products { get; set; }
}