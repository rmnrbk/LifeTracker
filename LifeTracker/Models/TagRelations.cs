namespace LifeTracker.Models;

public class TagRelations
{
    public int TagId { get; set; }
    public List<int> DirectParentIds { get; set; } = new();
    public List<int> ReachableExceptParentsIds { get; set; } = new();
    public List<int> CanBeParentIds { get; set; } = new();
}