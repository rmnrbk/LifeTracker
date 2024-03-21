using System.ComponentModel.DataAnnotations;

namespace LifeTracker.Entity;

public class Activity
{
    public int ActivityId { get; set; }
    [Required] public string OwnerID { get; set; }
    public int TagId { get; set; }
    public Tag? Tag { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
}