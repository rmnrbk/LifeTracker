using LifeTracker.Models.BaseModels;
using Microsoft.EntityFrameworkCore;

namespace LifeTracker.DataAccess;

public class AppDbContext: DbContext
{
    
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Activity> Activities { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}