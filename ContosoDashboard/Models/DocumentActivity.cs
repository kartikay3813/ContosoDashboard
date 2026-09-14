using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class DocumentActivity
{
    [Key] public int DocumentActivityId { get; set; }
    public int? DocumentId { get; set; }
    [Required, MaxLength(255)] public string DocumentIdentity { get; set; } = string.Empty;
    [Required] public int ActorUserId { get; set; }
    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    [MaxLength(255)] public string? RequestIp { get; set; }
    [MaxLength(2000)] public string? RequestMetadata { get; set; }
    public DateTime RetainUntil { get; set; } = DateTime.UtcNow.AddMonths(12);
    public virtual Document? Document { get; set; }
    public virtual User ActorUser { get; set; } = null!;
}
