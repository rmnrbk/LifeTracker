using LifeTracker.Models.BaseModels;
using Microsoft.EntityFrameworkCore;

namespace LifeTracker.DataAccess;

public class EntityRepository : IEntityRepository 
{
    private readonly IDbContextFactory<AppDbContext> _ctxFactory;
    private string _ownerId;

    public EntityRepository(IDbContextFactory<AppDbContext> ctxFactory)
    {
        _ctxFactory = ctxFactory;
    }

    public void SetOwnerId(string ownerId)
    {
        _ownerId = ownerId;
    }
    
    public async Task<Tag> GetTagByIdAsync(int tagId)
    {
        var tags = await GetTagsAsync();
        return tags.First(t => t.Id == tagId);
    }

    public List<Tag> GetTags()
    {
        using var ctx = _ctxFactory.CreateDbContext();
        var tags = ctx.Tags
            .Where(t => t.OwnerID == _ownerId)
            .Include(t => t.Parents)
            .Include(t => t.Activities)
            .ToList();

        return tags;
    }
    
    public async Task<List<Tag>> GetTagsAsync()
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var tags = await ctx.Tags
            .Where(t => t.OwnerID == _ownerId)
            .Include(t => t.Parents)
            .Include(t => t.Activities)
            .ToListAsync();

        return tags;
    }
    
    public List<Activity> GetActivities()
    {
        using var ctx = _ctxFactory.CreateDbContext();
        var activities = ctx.Activities
            .Where(a => a.OwnerID == _ownerId)
            .Include(a => a.Tag)
            .ToList();

        return activities;
    }
    
    public async Task<List<Activity>> GetActivitiesAsync()
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var activities = await ctx.Activities
            .Where(a => a.OwnerID == _ownerId)
            .Include(a => a.Tag)
            .ToListAsync();

        return activities;
    }
    
    public async Task<List<Activity>> GetActivitiesByTagIdAsync(int tagId)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var activities = await ctx.Activities
            .Where(a => a.OwnerID == _ownerId && a.TagId == tagId)
            .Include(a => a.Tag)
            .ToListAsync();

        return activities;
    }

    public async Task CreateTagAsync(Tag tag)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        tag.OwnerID = _ownerId;
        ctx.Tags.Add(tag);
        await ctx.SaveChangesAsync();
    }

    public async Task CreateActivityAsync(Activity activity)
    {
        activity.OwnerID = _ownerId;
        
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        ctx.Activities.Add(activity);
        await ctx.SaveChangesAsync();
    }

    public async Task EditActivityAsync(Activity editedActivity)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        ctx.Entry(editedActivity).State = EntityState.Modified;
        await ctx.SaveChangesAsync();
    }

    public async Task EditTagAsync(Tag editedTag)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        ctx.Entry(editedTag).State = EntityState.Modified;
        await ctx.SaveChangesAsync();
    }
    
    public async Task DeleteTagAsync(int tagId)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var tag = await ctx.Tags.FindAsync(tagId);
        if (tag == null)
        {
            return;
        }

        ctx.Tags.Remove(tag);
        await ctx.SaveChangesAsync();
    }
    
    public async Task DeleteActivityAsync(int activityId)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var activity = await ctx.Activities.FindAsync(activityId);
        if (activity == null)
        {
            return;
        }

        ctx.Activities.Remove(activity);
        await ctx.SaveChangesAsync();
    }
}