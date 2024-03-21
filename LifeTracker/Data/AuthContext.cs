using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LifeTracker.Data;

public class AuthContext: IdentityDbContext
{
    public AuthContext(DbContextOptions<AuthContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseNpgsql("Host=db;Port=5432;Database=authdb;Username=relfuser;Password=relfpassword");
    //     AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    //     // optionsBuilder.UseSqlite("Data Source=auth.db");
    // }
}