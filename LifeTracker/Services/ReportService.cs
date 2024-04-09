using LifeTracker.Mappers;
using LifeTracker.Models;
using LifeTracker.Models.BaseModels;

namespace LifeTracker.Services;

public class ReportService
{
    private readonly DisplayedTagMapper _mapper;
    private List<DisplayedTag> DisplayedTags { get; set; } = new(); 
    private List<DisplayedTag> FilteredTags { get; set; } = new();
    
    private static readonly string[] BackgroundColors = 
    {
        "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd", "#8c564b",
        "#e377c2", "#7f7f7f", "#bcbd22", "#17becf", "#aec7e8", "#ffbb78",
        "#98df8a", "#ff9896", "#c5b0d5", "#c49c94", "#f7b6d2", "#c7c7c7",
        "#dbdb8d", "#9edae5", "#393b79", "#637939", "#8c6d31", "#843c39",
        "#7b4173", "#5254a3", "#637b8d", "#8ca252", "#b5cf6b", "#cedb9c"
    };

    public ReportService(DisplayedTagMapper mapper)
    {
        _mapper = mapper;
    }
    
    public void Initialize(List<Tag> tags, List<Activity> activities, DateTime fromDateTime, DateTime toDateTime)
    {
        DisplayedTags = _mapper.FromTagsAndActivities(tags, activities, fromDateTime, toDateTime);
        var directlyActiveIds = GetDirectlyActiveIds();
        FilteredTags = new (DisplayedTags);
        FilteredTags.RemoveAll(dt => !directlyActiveIds.Contains(dt.Tag.Id));

        var filteredTagIds = FilteredTags.Select(dt => dt.Tag.Id).ToList();
    }
    
    public List<int> GetDirectlyActiveIds()
    {
        return DisplayedTags
            .Where(dt => dt.LastStartTime != null)
            .Select(dt => dt.Tag.Id)
            .ToList(); 
    }

    public List<DisplayedTag> GetDisplayedTags() => DisplayedTags;
    public List<DisplayedTag> GetFilteredTags() => FilteredTags;
    
    public List<string> GetTagsNames()
    {
        return FilteredTags
            .Select(dt => dt.Tag.Name)
            .ToList();
    }

    public List<double> GetData()
    {
        return FilteredTags
            .Select(dt => dt.TotalSecondsSpent)
            .Select(ToDoubleHours)
            .ToList();
    }
    
    private double ToDoubleHours(int secondsSpent)
    {
        int hours = secondsSpent / 3600;
        int minutes = (secondsSpent % 3600) / 60;
        return (double)hours + (double)minutes / 100;

        // double hours = Math.Round(secondsSpent / 3600.0, 2);
        // return hours;
    }
    
    public List<string> GetRandomBackgroundColors(int dataLabelsCount)
    {
        var colors = new List<string>();
        for (var index = 0; index < dataLabelsCount; index++)
        {
            colors.Add(BackgroundColors![index]);
        }

        return colors;
    }

    public void FilterTags(List<int> selectedTagsIds)
    {
        FilteredTags = new(DisplayedTags
            .Where(dt => selectedTagsIds.Contains(dt.Tag.Id)));
    }

    public List<int> GetNonZeroSecondsSpentIds()
    {
        return DisplayedTags
            .Where(dt => dt.TotalSecondsSpent > 0)
            .Select(dt => dt.Tag.Id)
            .ToList();
    }
}