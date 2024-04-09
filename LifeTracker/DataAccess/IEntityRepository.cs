using LifeTracker.Models.BaseModels;

namespace LifeTracker.DataAccess;

public interface IEntityRepository
{
    void SetOwnerId(string ownerId);
    Task<Tag> GetTagByIdAsync(int tagId);
    List<Tag> GetTags();
    Task<List<Tag>> GetTagsAsync();
    List<Activity> GetActivities();
    Task<List<Activity>> GetActivitiesAsync();
    Task<List<Activity>> GetActivitiesByTagIdAsync(int tagId);
    Task CreateTagAsync(Tag tag);
    Task CreateActivityAsync(Activity activity);
    Task EditActivityAsync(Activity editedActivity);
    Task EditTagAsync(Tag editedTag);
    Task DeleteTagAsync(int tagId);
    Task DeleteActivityAsync(int activityId);
}