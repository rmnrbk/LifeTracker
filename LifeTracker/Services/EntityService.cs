using LifeTracker.DataAccess;
using LifeTracker.Models.BaseModels;
using Microsoft.AspNetCore.Components.Authorization;

namespace LifeTracker.Services;

public class EntityService : BaseService
{
    private readonly IEntityRepository _entityRepository;

    public EntityService(IEntityRepository entityRepository, AuthenticationStateProvider authenticationStateProvider) : base(authenticationStateProvider)
    {
        _entityRepository = entityRepository;
    }
    
    public async Task InitializeAsync()
    {
        var ownerId = await GetCurrentUserIdAsync();
        _entityRepository.SetOwnerId(ownerId);
    }

    public List<Tag> GetTags()
    {
        return _entityRepository.GetTags();
    }
    
    public async Task<List<Tag>> GetTagsAsync()
    {
        return await _entityRepository.GetTagsAsync();
    }
    
    public async Task<List<Activity>> GetActivitiesAsync()
    {
        return await _entityRepository.GetActivitiesAsync();
    }
    
    public List<Activity> GetActivities()
    {
        return _entityRepository.GetActivities();
    }

    public async Task CreateTagAsync(string name, List<int> parentIds)
    {
        if (!IsNameValid(name) || NameAlreadyExist(name))
            return;
        
        var tag = new Tag
        {
            Name = name,
        };

        await _entityRepository.CreateTagAsync(tag, parentIds);
    }
    
    public async Task CreateActivityAsync(int tagId, DateTime start, DateTime? end=null)
    {
        var activity = new Activity
        {
            TagId = tagId,
            Start = start,
            End = end,
        };

        await _entityRepository.CreateActivityAsync(activity);
    }

    public async Task EditTagAsync(int tagId, string newTagName, List<int> newDirectParentIds)
    {
        if (!IsNameValid(newTagName))
            return;
        
        var newParents = new List<Tag>();
        
        var tags = await GetTagsAsync();
        var tag = tags.First(t => t.Id == tagId);
        
        foreach (var parentId in newDirectParentIds)
        {
            newParents.Add(tags.First(t => t.Id == parentId));
        }
        
        tag.Name = newTagName;
        tag.Parents = newParents;
        
        await _entityRepository.EditTagAsync(tag);
    }
    
    public async Task DeleteTagAsync(int tagId)
    {
        await _entityRepository.DeleteTagAsync(tagId);
    }

    public async Task<Activity> GetRunningActivityAsync(int tagId)
    {
        var activities = await _entityRepository.GetActivitiesByTagIdAsync(tagId);
        return activities.First(a => a.End is null);
    }
    
    public async Task EditActivityAsync(Activity editingActivity, int newTagId, DateTime newStartDatetime, DateTime? newEndDatetime)
    {
        var newTag = (await _entityRepository.GetTagsAsync())
            .First(t => t.Id == newTagId);
        
        var newActivity = new Activity
        {
            Id = editingActivity.Id,
            OwnerID = editingActivity.OwnerID,
            Tag = newTag,
            TagId = newTagId,
            Start = newStartDatetime,
            End = newEndDatetime,
        };
        
        await _entityRepository.EditActivityAsync(newActivity);
    }
    
    public async Task FinishRunningActivityAsync(int tagId, DateTime endDateTime)
    {
        var runningActivity = await GetRunningActivityAsync(tagId);
        await EditActivityAsync(runningActivity, runningActivity.TagId, runningActivity.Start, endDateTime);
    }
    
    public async Task DeleteActivityAsync(int editingActivityId)
    {
        await _entityRepository.DeleteActivityAsync(editingActivityId);
    }
    
    public async Task<List<int>> GetAllParentsAndChildrenIdsAsync(int tagId)
    {
        var tag = await _entityRepository.GetTagByIdAsync(tagId);
        var parentsIds = tag.GetAllParents().Select(t => t.Id);
        var childrenIds = tag.GetAllChildren().Select(t => t.Id);

        return parentsIds.Concat(childrenIds).ToList();
    }
    
    private bool IsNameValid(string name)
    {
        return name.Length >= 2;
    }

    private bool NameAlreadyExist(string name)
    {
        var tagNames = _entityRepository.GetTags().Select(t => t.Name);
        return tagNames.Contains(name);
    }
    
    public bool AreStartEndDatesValid(DateTime newStartDatetime, DateTime? newEndDatetime)
    {
        // TODO: validate properly
        if (newStartDatetime >= newEndDatetime)
            return false;
        if (Math.Abs((newStartDatetime - DateTime.Now).TotalDays) > 7)
            return false;
        if (newEndDatetime is not null &&
            Math.Abs((newEndDatetime.Value - newStartDatetime).TotalDays) > 7)
            return false;
        
        return true;
    }
}