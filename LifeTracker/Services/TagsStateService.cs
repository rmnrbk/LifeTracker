using System.Security.Claims;
using LifeTracker.Data;
using LifeTracker.Entity;
using LifeTracker.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LifeTracker.Services;

public class TagsStateService
{
    // private readonly HttpClient _httpClient;
    // private readonly NavigationManager _navigationManager;
    private readonly TagsCRUDService _tagsCRUDService;
    
    private string UserId { get; set; }
    public List<DisplayedTag> ActiveDisplayedTags { get; set; } = new();
    public List<DisplayedTag> DisplayedTags { get; private set; } = new();
    public List<Tag> Tags { get; private set; } = new();
    public List<Activity> Activities { get; private set; } = new();

    // public TagsStateService(HttpClient httpClient, NavigationManager navigationManager, TagsCRUDService tagsCrudService)
    public TagsStateService(TagsCRUDService tagsCrudService)
    {
        // _httpClient = httpClient;
        // _navigationManager = navigationManager;
        _tagsCRUDService = tagsCrudService;
    }

    public async Task SetupTagsAndActivities()
    {
        await _tagsCRUDService.InitializeAsync();
        
        Tags = await _tagsCRUDService.GetTags();
        Activities = await _tagsCRUDService.GetActivities();
        DisplayedTags = MapTagsAndActivities(Tags, Activities);
        ActiveDisplayedTags = DisplayedTags
            .Where(dt => dt.Status == DisplayedTagStatus.DirectlyActive).ToList();
    }

    public async Task CreateTag(string name, List<int> parentIds)
    {
        if (!IsNameValid(name) || NameAlreadyExist(name))
            return;

        await _tagsCRUDService.CreateTag(name, parentIds);
    }

    public async Task EditTag(int editingTagId, string newTagName, List<int> newDirectParentIds)
    {
        if (!IsNameValid(newTagName))
            return;

        await _tagsCRUDService.EditTagAsync(editingTagId, newTagName, newDirectParentIds);

        // var response = await _httpClient.PutAsJsonAsync(_navigationManager.BaseUri + "api/Tags/" + editingTagId, tag);
    }

    public async Task Toggle(DisplayedTag displayedTag)
    {
        await ToggleActivity(displayedTag);
        DisplayChanges(displayedTag);
    }
    
    private async Task ToggleActivity(DisplayedTag displayedTag)
    {
        if (displayedTag.Status == DisplayedTagStatus.Inactive)
        {
            await _tagsCRUDService.CreateActivity(displayedTag.Tag.TagId, DateTime.Now);
            // await _httpClient.PostAsJsonAsync(_navigationManager.BaseUri + "api/Activity", activity);
        }
        else
        {
            await _tagsCRUDService.FinishRunningActivity(displayedTag.Tag.TagId);
            // var activities = await _httpClient.GetFromJsonAsync<List<Activity>>(_navigationManager.BaseUri + "api/Activity/");
            // var currActivity = activities?
            //     .FirstOrDefault(a => a.TagId == displayedTag.Tag.TagId && a.End == null);
            // if (currActivity == null)
            //     throw new NullReferenceException("Не смог найти активную активность!!!");
            // currActivity.End = DateTime.Now;
            //
            // await _httpClient.PutAsJsonAsync(_navigationManager.BaseUri + "api/Activity/" + currActivity.ActivityId, currActivity);
        }
    }
    
    private void DisplayChanges(DisplayedTag displayedTag)
    {
        var allParentsIds = displayedTag.Tag.GetAllParents()
            .Select(t => t.TagId)
            .ToList();
        var allParents = DisplayedTags
            .Where(t => allParentsIds.Contains(t.Tag.TagId))
            .ToList();
        
        var allChildrenIds = displayedTag.Tag.GetAllChildren()
            .Select(t => t.TagId)
            .ToList();
        var allChildren = DisplayedTags
            .Where(t => allChildrenIds.Contains(t.Tag.TagId))
            .ToList();

        if (displayedTag.Status == DisplayedTagStatus.DirectlyActive)
        {
            displayedTag.DisableTimer();
            displayedTag.Status = DisplayedTagStatus.Inactive;
            displayedTag.LastEndTime = DateTime.Now;
            allParents.ForEach(t =>
            {
                t.ChildrenActivated -= 1;
                if (t.ChildrenActivated == 0)
                {
                    t.Status = DisplayedTagStatus.Inactive;
                    t.DisableTimer();
                }
            });
            allChildren.ForEach(t => t.Status = DisplayedTagStatus.Inactive);
            
            ActiveDisplayedTags.Remove(displayedTag);
        }
        else
        {
            displayedTag.EnableTimer();
            displayedTag.Status = DisplayedTagStatus.DirectlyActive;
            displayedTag.LastStartTime = DateTime.Now;
            displayedTag.LastEndTime = null;
            allParents.ForEach(t =>
            {
                t.ChildrenActivated += 1;
                t.Status = DisplayedTagStatus.DependentlyActive;
                t.EnableTimer();
            });
            allChildren.ForEach(t => t.Status = DisplayedTagStatus.DependentlyDisabled);
            
            ActiveDisplayedTags.Add(displayedTag);
        }
    }
    
    public List<int> GetAllParentsAndChildrenIds(int tagId)
    {
        var tag = DisplayedTags
            .First(dt => dt.Tag.TagId == tagId)
            .Tag;
        var ids = tag.GetAllParents()
            .Select(t => t.TagId)
            .ToList();
        ids.AddRange(tag.GetAllChildren()
            .Select(t => t.TagId)
            .ToList());

        return ids;
    }
    
    public void SetOnChangeHandler(Action onChangeHandler)
    {
        DisplayedTag.OnChange += onChangeHandler;
    }

    private void SetupReachableTagIds(TagRelations relations)
    {
        var tags = DisplayedTags.Select(dt => dt.Tag).ToList();
        var oldEdges = EdgesMapper.MapTagsToEdges(tags);
        var newEdges = UpdateParentEdges(oldEdges, relations);

        var reachableExceptParentsIds = FindReachableNodes(newEdges, relations.TagId);
        
        // Exclude direct parents and self
        reachableExceptParentsIds.RemoveAll(id => relations.DirectParentIds.Contains(id) || id == relations.TagId);
        
        relations.ReachableExceptParentsIds = reachableExceptParentsIds;
    }

    private List<(int, int)> UpdateParentEdges(List<(int, int)> oldEdges, TagRelations relations)
    {
        var newEdges = new List<(int, int)>(oldEdges);
        // Remove all edges from editing tag to parents
        newEdges.RemoveAll(e => e.Item2 == relations.TagId);
        
        // Add selected edges from editing tag to parents
        var newDirectParentEdges = relations.DirectParentIds
            .Select(pId => (pId, relations.TagId))
            .ToList();
        newEdges.AddRange(newDirectParentEdges);

        return newEdges;
    }

    private void SetupCanBeParentIds(TagRelations relations)
    {
        // All except reachable nodes and self
        relations.CanBeParentIds = DisplayedTags
            .Select(dt => dt.Tag.TagId)
            .Where(pId =>
                !relations.DirectParentIds.Contains(pId) &&
                !relations.ReachableExceptParentsIds.Contains(pId) &&
                relations.TagId != pId)
            .ToList();
    }

    private bool IsNameValid(string name)
    {
        if (name.Length < 2)
            return false;
        
        return true;
    }

    private bool NameAlreadyExist(string name)
    {
        if (DisplayedTags.Select(dt => dt.Tag.Name).Contains(name))
            return true;
        return false;
    }

    private List<DisplayedTag> MapTagsAndActivities(List<Tag> tags, List<Activity> activities)
    {
        // return DisplayedTagMapper.MapDisplayedTags(tags, activities);
        return DisplayedTagMapper.FromTagsAndTodayActivities(tags, activities, true);
    }
    
    private static List<int> FindReachableNodes(List<(int, int)> edges, int startingNode)
    {
        Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
        foreach ((int node1, int node2) in edges)
        {
            if (!graph.ContainsKey(node1))
                graph[node1] = new List<int>();
            if (!graph.ContainsKey(node2))
                graph[node2] = new List<int>();

            graph[node1].Add(node2);
            graph[node2].Add(node1);
        }

        HashSet<int> visited = new HashSet<int>();
        BFS(startingNode, graph, visited);

        return visited.ToList();
    }
    
    private static void BFS(int startingNode, Dictionary<int, List<int>> graph, HashSet<int> visited)
    {
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startingNode);
        visited.Add(startingNode);

        while (queue.Count > 0)
        {
            int currentNode = queue.Dequeue();
            if (!graph.TryGetValue(currentNode, out var value)) continue;
            foreach (int neighbor in value)
            {
                if (visited.Contains(neighbor)) continue;
                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
    }

    public async Task DeleteTag(int editingTagId)
    {
        // await _httpClient.DeleteAsync(_navigationManager.BaseUri + "api/Tags/" + editingTagId);
        await _tagsCRUDService.DeleteTagAsync(editingTagId);
    }

    public async Task<Activity> GetCurrActivity(int tagId)
    {
        // var activities = await _httpClient.GetFromJsonAsync<List<Activity>>(_navigationManager.BaseUri + "api/Activity");
        var activities = await _tagsCRUDService.GetActivities();
        
        return activities.First(a =>
            a.TagId == tagId &&
            a.End is null);
    }
    
    public async Task EditActivity(Activity editingActivity, int newTagId, DateTime newStartDatetime, DateTime? newEndDatetime)
    {
        await _tagsCRUDService.EditActivityAsync(editingActivity, newTagId, newStartDatetime, newEndDatetime);

        // var response = await _httpClient.PutAsJsonAsync(_navigationManager.BaseUri + "api/Activity/" + editingActivity.ActivityId, newActivity);
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

    public async Task DeleteActivity(int editingActivityId)
    {
        // await _httpClient.DeleteAsync(_navigationManager.BaseUri + "api/Activity/" + editingActivityId);
        await _tagsCRUDService.DeleteActivityAsync(editingActivityId);
    }
}