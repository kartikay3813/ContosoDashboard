using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class Team
{
    [Key] public int TeamId { get; set; }
    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<TeamMembership> Memberships { get; set; } = new List<TeamMembership>();
    public virtual ICollection<DocumentShare> DocumentShares { get; set; } = new List<DocumentShare>();
}

public class TeamMembership
{
    [Key] public int TeamMembershipId { get; set; }
    [Required] public int TeamId { get; set; }
    [Required] public int UserId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTeamLead { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public virtual Team Team { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
