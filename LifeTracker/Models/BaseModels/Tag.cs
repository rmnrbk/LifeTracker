using System.ComponentModel.DataAnnotations;

namespace LifeTracker.Models.BaseModels;

public class Tag
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string OwnerID { get; set; }
    public required string Name { get; set; }
    public List<Tag> Parents { get; set; } = new();
    public List<Tag> Children { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
    
    public List<Tag> GetAllParents()
    {
        var allParents = new List<Tag>();
        foreach (var parent in Parents)
        {
            allParents.Add(parent);
            allParents.AddRange(parent.GetAllParents());
        }
        return allParents;
    }
    
    public List<Tag> GetAllChildren()
    {
        var allChildren = new List<Tag>();
        foreach (var child in Children)
        {
            if (child != null)
            {
                allChildren.Add(child);
                allChildren.AddRange(child.GetAllChildren());
            }
        }
        return allChildren;
    }

    public List<Tag> GetTopParents()
    {
        return GetAllParents()
            .Where(t => t.Parents.Count == 0)
            .ToList();
    }
}