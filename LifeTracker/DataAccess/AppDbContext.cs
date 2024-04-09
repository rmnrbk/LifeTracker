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
    
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseNpgsql("Host=db;Port=5432;Database=relfdb;Username=relfuser;Password=relfpassword");
    //     AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    //     // optionsBuilder.UseSqlite("Data Source=tag.db");
    // }
}