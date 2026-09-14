using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key] public int DocumentShareId { get; set; }
    [Required] public int DocumentId { get; set; }
    public int? UserId { get; set; }
    public int? TeamId { get; set; }
    [Required] public int GrantedByUserId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public virtual Document Document { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual Team? Team { get; set; }
    public virtual User GrantedByUser { get; set; } = null!;
    [NotMapped] public bool HasExactlyOneTarget => (UserId.HasValue ? 1 : 0) + (TeamId.HasValue ? 1 : 0) == 1;
}
