using LifeTracker.Mappers;
using LifeTracker.Models;
using LifeTracker.Models.BaseModels;

namespace LifeTracker.Services;

public class DisplayedTagService
{
    private readonly DisplayedTagMapper _mapper;
    public List<DisplayedTag> DTags { get; private set; } = new(); 
    public List<DisplayedTag> ActiveDTags { get; set; } = new();

    public DisplayedTagService(DisplayedTagMapper mapper)
    {
        _mapper = mapper;
    }
    
    public void Initialize(List<Tag> tags, List<Activity> activities)
    {
        DTags = _mapper.MapTagsAndActivities(tags, activities);
        ActiveDTags = DTags
            .Where(dt => dt.Status == DisplayedTagStatus.DirectlyActive)
            .ToList();
    }
    
    public void SetOnChangeHandler(Action onChangeHandler)
    {
        DisplayedTag.OnChange += onChangeHandler;
    }

    public void Update(int tagId)
    {
        var hDTag = DTags.First(t => t.Tag.Id == tagId);
        Update(hDTag);
    }
    
    public void Update(DisplayedTag hDTag)
    {
        var allParentsIds = hDTag.Tag.GetAllParents()
            .Select(t => t.Id)
            .ToList();
        var allParents = DTags
            .Where(t => allParentsIds.Contains(t.Tag.Id))
            .ToList();
        
        var allChildrenIds = hDTag.Tag.GetAllChildren()
            .Select(t => t.Id)
            .ToList();
        var allChildren = DTags
            .Where(t => allChildrenIds.Contains(t.Tag.Id))
            .ToList();

        if (hDTag.Status == DisplayedTagStatus.DirectlyActive)
        {
            hDTag.DisableTimer();
            hDTag.Status = DisplayedTagStatus.Inactive;
            hDTag.LastEndTime = DateTime.Now;
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
            
            ActiveDTags.Remove(hDTag);
        }
        else
        {
            hDTag.EnableTimer();
            hDTag.Status = DisplayedTagStatus.DirectlyActive;
            hDTag.LastStartTime = DateTime.Now;
            hDTag.LastEndTime = null;
            allParents.ForEach(t =>
            {
                t.ChildrenActivated += 1;
                t.Status = DisplayedTagStatus.DependentlyActive;
                t.EnableTimer();
            });
            allChildren.ForEach(t => t.Status = DisplayedTagStatus.DependentlyDisabled);
            
            ActiveDTags.Add(hDTag);
        }
    }
}