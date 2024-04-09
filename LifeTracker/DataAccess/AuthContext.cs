using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LifeTracker.DataAccess;

public class AuthContext: IdentityDbContext
{
    public AuthContext(DbContextOptions<AuthContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}