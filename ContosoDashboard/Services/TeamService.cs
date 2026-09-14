using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public interface ITeamService
{
    Task<List<Team>> GetTeamsAsync(int userId);
    Task<bool> SetMembershipAsync(int actorUserId, int teamId, int userId, bool active, bool teamLead);
}

public sealed class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;
    public TeamService(ApplicationDbContext context) => _context = context;
    public Task<List<Team>> GetTeamsAsync(int userId) => _context.Teams.Include(t => t.Memberships).Where(t => t.Memberships.Any(m => m.UserId == userId && m.IsActive) || _context.Users.Any(u => u.UserId == userId && u.Role == UserRole.Administrator)).ToListAsync();
    public async Task<bool> SetMembershipAsync(int actorUserId, int teamId, int userId, bool active, bool teamLead)
    {
        var actor = await _context.Users.FindAsync(actorUserId);
        if (actor?.Role is not (UserRole.TeamLead or UserRole.ProjectManager or UserRole.Administrator)) return false;
        var membership = await _context.TeamMemberships.FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);
        if (membership is null) _context.TeamMemberships.Add(membership = new TeamMembership { TeamId = teamId, UserId = userId });
        membership.IsActive = active;
        membership.IsTeamLead = teamLead;
        await _context.SaveChangesAsync();
        return true;
    }
}
