using System.ComponentModel.DataAnnotations;

namespace LifeTracker.Models.BaseModels;

public class Activity
{
    public int Id { get; set; }
    [Required] public string OwnerID { get; set; }
    public int TagId { get; set; }
    public Tag? Tag { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
}