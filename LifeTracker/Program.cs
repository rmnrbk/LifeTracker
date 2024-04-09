using LifeTracker.DataAccess;
using LifeTracker.Mappers;
using LifeTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazorBootstrap();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var authCs = builder.Configuration.GetConnectionString("AuthDb");
var tagsCs = builder.Configuration.GetConnectionString("TagsDb");
builder.Services.AddDbContext<AuthContext>(options => options.UseNpgsql(authCs));
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 5;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        
        options.SignIn.RequireConfirmedEmail = false;
        
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AuthContext>();

builder.Services.AddHttpClient();

builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(tagsCs));
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(tagsCs));

builder.Services.AddScoped<DisplayedTagMapper>();
builder.Services.AddScoped<DisplayedTagService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<EntityService>();
builder.Services.AddScoped<IEntityRepository, EntityRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Initialize the database
if (!app.Environment.IsProduction())
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    db.Database.EnsureDeleted();
    if (db.Database.EnsureCreated())
    {
        await SeedData.Initialize(db, userManager);
    }
}

app.Run();