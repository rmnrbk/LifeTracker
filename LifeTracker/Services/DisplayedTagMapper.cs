using LifeTracker.Entity;
using LifeTracker.Models;

namespace LifeTracker.Services;

public static class DisplayedTagMapper
{
    public static List<DisplayedTag> FromTagsAndTodayActivities(List<Tag> tags, List<Activity> activities, bool enableTimer=false)
    {
        return FromTagsAndActivities(tags, activities, DateTime.Today, 
            DateTime.Today + TimeSpan.FromDays(1), enableTimer);
    }
    
    public static List<DisplayedTag> FromTagsAndActivities(List<Tag> tags, List<Activity> activities, 
        DateTime fromDateTime, DateTime toDateTime, bool enableTimer=false)
    {
        var intervalActivities = ActivitiesFromInterval(activities, fromDateTime, toDateTime);
        var displayedTags = new List<DisplayedTag>();

        var runningActivities = intervalActivities
            .Where(a => a.End == null)
            .ToList();
        
        // Find running tags and its parents and children
        var directlyActiveTagIds = runningActivities.Select(a => a.TagId).ToList();
        var dependentlyActiveTagIds = new List<int>();
        var dependentlyDisabledTagIds = new List<int>();
        
        runningActivities.ForEach(a =>
        {
            var relatedTag = tags.First(t => t.TagId == a.TagId);
            var currParentsTagIds = relatedTag
                .GetAllParents()
                .Select(t => t.TagId)
                .ToList();
            var currChildrenTagIds = relatedTag
                .GetAllChildren()
                .Select(t => t.TagId)
                .ToList();
            
            dependentlyActiveTagIds.AddRange(currParentsTagIds);
            dependentlyDisabledTagIds.AddRange(currChildrenTagIds);
        });
        
        foreach (var tag in tags)
        {
            var status = DisplayedTagStatus.Inactive;
            if (directlyActiveTagIds.Contains(tag.TagId))
                status = DisplayedTagStatus.DirectlyActive;
            else if (dependentlyActiveTagIds.Contains(tag.TagId))
                status = DisplayedTagStatus.DependentlyActive;
            else if (dependentlyDisabledTagIds.Contains(tag.TagId))
                status = DisplayedTagStatus.DependentlyDisabled;

            var lastSecondsSpent = 0;
            var totalSecondsSpent = 0;
            var childrenActivated = 0;
            switch (status)
            {
                case DisplayedTagStatus.DirectlyActive:
                {
                    var start = runningActivities
                        .First(a => a.TagId == tag.TagId)
                        .Start;
                    lastSecondsSpent = (int)((DateTime.Now < toDateTime ? DateTime.Now : toDateTime) - start).TotalSeconds;
                    totalSecondsSpent = CalcTotalDuration(tag, intervalActivities, fromDateTime, toDateTime) + lastSecondsSpent;
                    break;
                }
                case DisplayedTagStatus.DependentlyActive:
                {
                    childrenActivated = tag
                        .GetAllChildren()
                        .Count(t => directlyActiveTagIds.Contains(t.TagId));
                    
                    var earliestActiveChildTag = tag    // TODO: calculate using runningActivities, not tags
                        .GetAllChildren()
                        .Where(t => directlyActiveTagIds.Contains(t.TagId))
                        .Aggregate((earliest, current) =>
                            runningActivities.First(a => a.TagId == current.TagId).Start < 
                            runningActivities.First(a => a.TagId == earliest.TagId).Start ? 
                                current : earliest);
                
                    var start = runningActivities
                        .First(a => a.TagId == earliestActiveChildTag.TagId)
                        .Start;
                    lastSecondsSpent = (int)((DateTime.Now < toDateTime ? DateTime.Now : toDateTime) - start).TotalSeconds;
                    totalSecondsSpent = CalcTotalDuration(tag, intervalActivities, fromDateTime, toDateTime) + lastSecondsSpent;
                    break;
                }
                default:
                {
                    var childIds = tag.GetAllChildren().Select(t => t.TagId);
                    var childActivities = intervalActivities
                        .Where(a => childIds.Contains(a.TagId))
                        .ToList();
                    var selfActivities = intervalActivities
                        .Where(a => a.TagId == tag.TagId)
                        .ToList();
                    
                    Activity? latestChildActivity = childActivities.Any() ?
                        childActivities.Aggregate((latest, current) =>
                            current.Start > latest.Start ? current : latest)
                        : null;
                    
                    Activity? latestSelfActivity = selfActivities.Any() ?
                        selfActivities.Aggregate((latest, current) =>
                            current.Start > latest.Start ? current : latest)
                        : null;

                    if (latestSelfActivity == null && latestChildActivity == null)
                        break;
                    
                    Activity latestActivity;
                    if (latestSelfActivity != null && latestChildActivity != null)
                    {
                        latestActivity = latestSelfActivity.Start > latestChildActivity.Start
                            ? latestSelfActivity
                            : latestChildActivity;
                    }
                    else
                    {
                        latestActivity = latestSelfActivity ?? latestChildActivity!;
                    }
                    
                    lastSecondsSpent = (int)(latestActivity.End!.Value - latestActivity.Start).TotalSeconds;
                    totalSecondsSpent = CalcTotalDuration(tag, intervalActivities, fromDateTime, toDateTime);
                    
                    // Novoe
                    int diffOutsideOfDatesInterval = int.Max(
                        (int)(fromDateTime - latestActivity.Start).TotalSeconds, 
                        0);
                    lastSecondsSpent -= diffOutsideOfDatesInterval;
                    totalSecondsSpent -= diffOutsideOfDatesInterval;
                    // Novoe
                    break;
                }
            }
            

            var tagDirectActivities = intervalActivities
                .Where(a => 
                    a.TagId == tag.TagId  
                    // && a.Start.Date == DateTime.Now.Date
                    )
                .ToList();
            
            var lastTagDirectActivity = tagDirectActivities.Any()
                ? tagDirectActivities.Aggregate((max, current) =>
                    current.Start > max.Start ? current : max)
                : null;

            DateTime? lastStartTime = null;
            DateTime? lastEndTime = null;
            if (lastTagDirectActivity != null)
            {
                lastStartTime = lastTagDirectActivity.Start;
                lastEndTime = lastTagDirectActivity.End;
            }
            
            var displayedTag = new DisplayedTag
            {
                Tag = tag,
                LastSecondsSpent = lastSecondsSpent,
                TotalSecondsSpent = totalSecondsSpent,
                LastStartTime = lastStartTime,
                LastEndTime = lastEndTime,
                Status = status,
                ChildrenActivated = childrenActivated
            };
            displayedTags.Add(displayedTag);
        }

        if (enableTimer)
        {
            displayedTags.ForEach(dt =>
            {
                if (directlyActiveTagIds.Contains(dt.Tag.TagId) ||
                    dependentlyActiveTagIds.Contains(dt.Tag.TagId))
                {
                    dt.EnableTimer(false);
                }
            });
        }
        
        return displayedTags
            .OrderByDescending(t => t.TotalSecondsSpent)
            .ToList();
    }

    private static List<Activity> ActivitiesFromInterval(List<Activity> activities, DateTime fromDateTime, DateTime toDateTime)
    {
        if (fromDateTime > DateTime.Now)
            return new List<Activity>();
        
        var actsFromInterval = new List<Activity>(activities);
        actsFromInterval.RemoveAll(a => a.Start > toDateTime);
        actsFromInterval.RemoveAll(a =>
        {
            if (!a.End.HasValue)
                return false;
            return a.End.Value < fromDateTime;
        });
        return actsFromInterval;
    }

    // public static List<DisplayedTag> MapDisplayedTags(List<Tag> tags, List<Activity> activities)
    // {
    //     var displayedTags = new List<DisplayedTag>();
    //     
    //     var runningActivities = activities
    //         .Where(a => a.End == null)
    //         .ToList();
    //     
    //     // Find running tags and its parents and children
    //     var directlyActiveTagIds = runningActivities.Select(a => a.TagId).ToList();
    //     var dependentlyActiveTagIds = new List<int>();
    //     var dependentlyDisabledTagIds = new List<int>();
    //     
    //     runningActivities.ForEach(a =>
    //     {
    //         var relatedTag = tags.First(t => t.TagId == a.TagId);
    //         var currParentsTagIds = relatedTag
    //             .GetAllParents()
    //             .Select(t => t.TagId)
    //             .ToList();
    //         var currChildrenTagIds = relatedTag
    //             .GetAllChildren()
    //             .Select(t => t.TagId)
    //             .ToList();
    //         
    //         dependentlyActiveTagIds.AddRange(currParentsTagIds);
    //         dependentlyDisabledTagIds.AddRange(currChildrenTagIds);
    //     });
    //     
    //     foreach (var tag in tags)
    //     {
    //         var status = DisplayedTagStatus.Inactive;
    //         if (directlyActiveTagIds.Contains(tag.TagId))
    //             status = DisplayedTagStatus.DirectlyActive;
    //         else if (dependentlyActiveTagIds.Contains(tag.TagId))
    //             status = DisplayedTagStatus.DependentlyActive;
    //         else if (dependentlyDisabledTagIds.Contains(tag.TagId))
    //             status = DisplayedTagStatus.DependentlyDisabled;
    //
    //         var currDuration = 0;
    //         var currChildActivated = 0;
    //         switch (status)
    //         {
    //             case DisplayedTagStatus.DirectlyActive:
    //             {
    //                 var start = runningActivities
    //                     .First(a => a.TagId == tag.TagId)
    //                     .Start;
    //                 currDuration = Convert.ToInt32((DateTime.Now - start).TotalSeconds);
    //                 break;
    //             }
    //             case DisplayedTagStatus.DependentlyActive:
    //             {
    //                 currChildActivated = tag
    //                     .GetAllChildren()
    //                     .Count(t => directlyActiveTagIds.Contains(t.TagId));
    //                 
    //                 var activeChildTag = tag
    //                     .GetAllChildren()
    //                     .First(t => directlyActiveTagIds.Contains(t.TagId));    // TODO: choose most early started, not random
    //             
    //                 var start = runningActivities
    //                     .First(a => a.TagId == activeChildTag.TagId)
    //                     .Start;
    //                 currDuration = Convert.ToInt32((DateTime.Now - start).TotalSeconds);
    //                 break;
    //             }
    //         }
    //
    //         var totalDuration = CalcTotalDuration(tag, activities) + currDuration;
    //
    //         var todayActivities = activities
    //             .Where(a => 
    //                 a.TagId == tag.TagId  &&
    //                 a.Start.Date == DateTime.Now.Date).ToList();
    //         
    //         var lastActivity = todayActivities.Any()
    //             ? todayActivities.Aggregate((max, current) =>
    //                 current.Start > max.Start ? current : max)
    //             : null;
    //
    //         DateTime? lastTodayStartTime = null;
    //         DateTime? lastTodayEndTime = null;
    //         if (lastActivity != null)
    //         {
    //             lastTodayStartTime = lastActivity.Start;
    //             lastTodayEndTime = lastActivity.End;
    //         }
    //         
    //         var displayedTag = new DisplayedTag
    //         {
    //             Tag = tag,
    //             LastSecondsSpent = currDuration,
    //             TotalSecondsSpent = totalDuration,
    //             LastStartTime = lastTodayStartTime,
    //             LastEndTime = lastTodayEndTime,
    //             Status = status,
    //             ChildrenActivated = currChildActivated
    //         };
    //         displayedTags.Add(displayedTag);
    //     }
    //     
    //     displayedTags.ForEach(dt =>
    //     {
    //         if (directlyActiveTagIds.Contains(dt.Tag.TagId) ||
    //             dependentlyActiveTagIds.Contains(dt.Tag.TagId))
    //         {
    //             dt.EnableTimer();
    //         }
    //     });
    //
    //     return displayedTags
    //         .OrderByDescending(t => t.TotalSecondsSpent)
    //         .ToList();
    // }

    private static int CalcTotalDuration(Tag tag, List<Activity> activities, DateTime fromDateTime, DateTime toDateTime)
    {
        var relatedActivities = GetRelatedActivities(tag, activities);
        relatedActivities.RemoveAll(a => a.End is null);
        
        // Sort activities by start time
        var sortedActivities = relatedActivities.OrderBy(a => a.Start).ToList();

        // Merge overlapping time spans
        var mergedTimeSpans = new List<(DateTime, DateTime?)>();
        foreach (var activity in sortedActivities)
        {
            if (mergedTimeSpans.Count == 0 || activity.Start > mergedTimeSpans.Last().Item2)
            {
                // Non-overlapping interval, add directly
                mergedTimeSpans.Add((activity.Start, activity.End));
            }
            else if (activity.End > mergedTimeSpans.Last().Item2)
            {
                // Overlapping interval, extend the end time
                mergedTimeSpans[^1] =
                    (mergedTimeSpans.Last().Item1, activity.End);
            }
        }

        // Calculate total time spent
        TimeSpan totalTime = TimeSpan.Zero;
        foreach (var interval in mergedTimeSpans)
        {
            totalTime += interval.Item2!.Value - interval.Item1;
        }

        return (int)totalTime.TotalSeconds;
    }

    private static List<Activity> GetRelatedActivities(Tag tag, List<Activity> activities)
    {
        var relatedTagIds = tag.GetAllChildren()
            .Select(t => t.TagId)
            .ToList();
        relatedTagIds.Add(tag.TagId);

        var relatedActivities = activities
            .Where(a => 
                relatedTagIds.Contains(a.TagId) 
                // && a.Start.Date == DateTime.Now.Date
                )
            .ToList();
        
        return relatedActivities;
    }

}