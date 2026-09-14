using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public interface IDocumentAuthorizationService
{
    IQueryable<Document> AccessibleDocuments(int userId);
    Task<bool> CanReadAsync(int userId, Document document);
    Task<bool> CanManageAsync(int userId, Document document);
    Task<bool> CanAccessTaskAsync(int userId, int taskId);
}

public sealed class DocumentAuthorizationService : IDocumentAuthorizationService
{
    private readonly ApplicationDbContext _context;
    public DocumentAuthorizationService(ApplicationDbContext context) => _context = context;

    public IQueryable<Document> AccessibleDocuments(int userId)
    {
        var isAdmin = _context.Users.Where(u => u.UserId == userId).Select(u => u.Role == UserRole.Administrator);
        return _context.Documents.Where(d => d.ScanStatus == DocumentScanStatus.Clean || d.ScanStatus == DocumentScanStatus.Replaced)
            .Where(d => isAdmin.Any(a => a) || d.UploadedByUserId == userId
                || (d.ProjectId.HasValue && (d.Project!.ProjectManagerId == userId || d.Project.ProjectMembers.Any(pm => pm.UserId == userId)))
                || d.Shares.Any(s => s.RevokedAt == null && ((s.UserId == userId) || (s.TeamId.HasValue && s.Team!.Memberships.Any(m => m.UserId == userId && m.IsActive)))));
    }

    public async Task<bool> CanReadAsync(int userId, Document document)
        => await AccessibleDocuments(userId).AnyAsync(d => d.DocumentId == document.DocumentId);

    public async Task<bool> CanManageAsync(int userId, Document document)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user?.Role == UserRole.Administrator || document.UploadedByUserId == userId) return true;
        if (document.ProjectId.HasValue && await _context.Projects.AnyAsync(p => p.ProjectId == document.ProjectId && p.ProjectManagerId == userId)) return true;
        return document.ProjectId.HasValue && await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == document.ProjectId && pm.UserId == userId && pm.Role == "TeamLead");
    }

    public async Task<bool> CanAccessTaskAsync(int userId, int taskId)
        => await _context.Tasks.AnyAsync(t => t.TaskId == taskId && (t.AssignedUserId == userId || t.CreatedByUserId == userId || t.Project!.ProjectManagerId == userId || t.Project.ProjectMembers.Any(pm => pm.UserId == userId)));
}
