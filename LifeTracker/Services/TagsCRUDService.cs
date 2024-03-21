using System.Security.Claims;
using LifeTracker.Data;
using LifeTracker.Entity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LifeTracker.Services;

public class TagsCRUDService
{
    private readonly IDbContextFactory<TagStoreContext> _ctxFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    
    public string? UserId { get; set; }

    public TagsCRUDService(AuthenticationStateProvider authenticationStateProvider, IDbContextFactory<TagStoreContext> ctxFactory)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _ctxFactory = ctxFactory;
    }

    public async Task InitializeAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var userId = state.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return;
        UserId = userId;
    }

    public async Task<List<Tag>> GetTags()
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var tags = await ctx.Tags
            .Where(t => t.OwnerID == UserId)
            .Include(t => t.Parents)
            .Include(t => t.Activities)
            .ToListAsync();

        return tags;
    }
    
    public async Task<List<Activity>> GetActivities()
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var activities = await ctx.Activities
            .Where(a => a.OwnerID == UserId)
            .Include(a => a.Tag)
            .ToListAsync();

        return activities;
    }

    public async Task CreateTag(string name, List<int> parentIds)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var parents = ctx.Tags
            .Where(t => 
                t.OwnerID == UserId &&
                parentIds.Contains(t.TagId))
            .ToList();
        
        var tag = new Tag
        {
            Name = name,
            OwnerID = UserId!,
            Parents = parents
        };
        
        // tag.Parents.AddRange(parents);
        ctx.Tags.Add(tag);
        await ctx.SaveChangesAsync();
        // await _httpClient.PostAsJsonAsync(_navigationManager.BaseUri + "api/Tags", tag);
    }

    public async Task CreateActivity(int tagId, DateTime start, DateTime? end=null)
    {
        var activity = new Activity
        {
            TagId = tagId,
            OwnerID = UserId!,
            Start = start,
            End = end,
        };
        
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        ctx.Activities.Add(activity);
        await ctx.SaveChangesAsync();
    }

    public async Task FinishRunningActivity(int tagId)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var activity = (await GetActivities())
            .First(a => 
                a.OwnerID == UserId && 
                a.TagId == tagId &&
                a.End == null);
        activity.End = DateTime.Now;
        
        ctx.Entry(activity).State = EntityState.Modified;
        await ctx.SaveChangesAsync();
    }
    
    private async Task<bool> ActivityExists(int id)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        return (ctx.Activities?.Any(e => e.ActivityId == id)).GetValueOrDefault();
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

    public async Task EditActivityAsync(Activity editingActivity, int newTagId, DateTime newStartDatetime, DateTime? newEndDatetime)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var newTag = ctx.Tags.First(t => t.OwnerID == UserId && t.TagId == newTagId);
        var newActivity = new Activity
        {
            ActivityId = editingActivity.ActivityId,
            OwnerID = UserId!,
            Tag = newTag,
            TagId = newTagId,
            Start = newStartDatetime,
            End = newEndDatetime,
        };
        
        ctx.Entry(newActivity).State = EntityState.Modified;
        await ctx.SaveChangesAsync();
    }

    public async Task EditTagAsync(int editingTagId, string newTagName, List<int> newDirectParentIds)
    {
        await using var ctx = await _ctxFactory.CreateDbContextAsync();
        var newParents = ctx.Tags
            .Where(t => t.OwnerID == UserId && newDirectParentIds.Contains(t.TagId))
            .ToList();

        // var newTag = ctx.Tags.First(t => t.OwnerID == UserId && t.TagId == editingTagId);
        //
        // newTag.Name = newTagName;
        // newTag.Parents = newParents;
        
        var existingTag = ctx.Tags
            .Include(t => t.Parents)
            .Include(t => t.Children)
            .First(t => t.OwnerID == UserId && t.TagId == editingTagId);
        
        // ctx.Entry(existingTag).CurrentValues.SetValues(newTag);
        existingTag.Parents = newParents;
        existingTag.Name = newTagName;
        ctx.Entry(existingTag).State = EntityState.Modified;
        
        // // Update relationships (Parents) manually
        // existingTag.Parents.Clear(); // Clear existing parents
        // foreach (var parent in newTag.Parents)
        // {
        //     var parentTag = await ctx.Tags.FindAsync(parent.TagId);
        //     if (parentTag != null)
        //     {
        //         existingTag.Parents.Add(parentTag);
        //     }
        // }
        
        await ctx.SaveChangesAsync();
    }
}