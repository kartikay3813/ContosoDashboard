using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public sealed record DocumentUploadRequest(string Title, string Category, string FileName, string ContentType, long FileSize, Stream Content, string? Description = null, int? ProjectId = null, int? TaskId = null, IReadOnlyCollection<string>? Tags = null);
public sealed record DocumentSummary(int DocumentId, string Title, string Category, string OriginalFileName, string FileType, long FileSize, DateTime UploadedAt, int? ProjectId, int? TaskId, string? UploaderName);
public sealed record UploadResult(string FileName, bool Success, string? Error, DocumentSummary? Document);
public sealed record DocumentQuery(string? Search = null, string? Category = null, int? ProjectId = null, DateTime? From = null, DateTime? To = null, string Sort = "date");

public interface IDocumentService
{
    Task<UploadResult> UploadAsync(int userId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentSummary>> ListMyDocumentsAsync(int userId, DocumentQuery query);
    Task<IReadOnlyList<DocumentSummary>> ListProjectDocumentsAsync(int userId, int projectId, DocumentQuery query);
    Task<IReadOnlyList<DocumentSummary>> ListSharedDocumentsAsync(int userId, DocumentQuery query);
    Task<IReadOnlyList<DocumentSummary>> SearchAsync(int userId, DocumentQuery query);
    Task<DocumentSummary?> GetMetadataAsync(int userId, int documentId);
    Task<(Stream Content, string ContentType, string FileName)?> OpenForRetrievalAsync(int userId, int documentId);
    Task<bool> UpdateMetadataAsync(int userId, int documentId, string title, string category, string? description);
    Task<bool> ReplaceFileAsync(int userId, int documentId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int userId, int documentId);
    Task<bool> ShareAsync(int userId, int documentId, int? targetUserId, int? targetTeamId);
    Task<bool> RevokeShareAsync(int userId, int documentId, int shareId);
    Task<IReadOnlyList<DocumentSummary>> GetRecentUploadsAsync(int userId, int limit = 5);
    Task<int> GetDocumentCountAsync(int userId);
    Task<UploadResult> AttachToTaskAsync(int userId, int taskId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
}

public sealed class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly IMalwareScanner _scanner;
    private readonly IDocumentAuthorizationService _authorization;
    private readonly INotificationService _notifications;
    public DocumentService(ApplicationDbContext context, IFileStorageService storage, IMalwareScanner scanner, IDocumentAuthorizationService authorization, INotificationService notifications) => (_context, _storage, _scanner, _authorization, _notifications) = (context, storage, scanner, authorization, notifications);

    public async Task<UploadResult> UploadAsync(int userId, DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (validation != null) return Fail(request, validation);
        if (request.ProjectId.HasValue && !await CanProjectAsync(userId, request.ProjectId.Value)) return Fail(request, "You do not have access to this project.");
        if (request.TaskId.HasValue && (!await _authorization.CanAccessTaskAsync(userId, request.TaskId.Value) || !await _context.Tasks.AnyAsync(t => t.TaskId == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken))) return Fail(request, "The task and project do not match.");
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var key = $"{userId}/{request.ProjectId?.ToString() ?? "personal"}/{Guid.NewGuid():N}{extension}";
        var quarantineKey = Guid.NewGuid().ToString("N") + extension;
        try
        {
            await _storage.UploadAsync(request.Content, quarantineKey, request.ContentType, true, cancellationToken);
            await using var scanStream = await _storage.OpenReadAsync(quarantineKey, true, cancellationToken);
            var scan = await _scanner.ScanAsync(scanStream, request.ContentType, cancellationToken);
            if (scan != ScanResult.Clean)
            {
                await CleanupAsync(quarantineKey);
                return Fail(request, scan == ScanResult.Threat ? "The file was rejected by malware scanning." : "The malware scan failed.");
            }
            await using var staged = await _storage.OpenReadAsync(quarantineKey, true, cancellationToken);
            await _storage.UploadAsync(staged, key, request.ContentType, false, cancellationToken);
            var document = new Document { Title = request.Title.Trim(), Description = request.Description, Category = request.Category.Trim(), OriginalFileName = Path.GetFileName(request.FileName), StoragePath = key, FileType = request.ContentType, FileSize = request.FileSize, UploadedByUserId = userId, ProjectId = request.ProjectId, TaskId = request.TaskId, ScanStatus = DocumentScanStatus.Clean };
            foreach (var tag in request.Tags ?? Array.Empty<string>()) { var normalized = tag.Trim().ToLowerInvariant(); if (normalized.Length > 0) document.Tags.Add(new DocumentTag { TagText = normalized }); }
            _context.Documents.Add(document);
            _context.DocumentActivities.Add(new DocumentActivity { Document = document, DocumentIdentity = document.OriginalFileName, ActorUserId = userId, Action = "Upload" });
            await _context.SaveChangesAsync(cancellationToken);
            await _storage.DeleteAsync(quarantineKey, true, cancellationToken);
            if (request.ProjectId.HasValue)
            {
                var recipients = await _context.ProjectMembers.Where(pm => pm.ProjectId == request.ProjectId && pm.UserId != userId).Select(pm => pm.UserId).ToListAsync(cancellationToken);
                foreach (var recipient in recipients) await _notifications.CreateNotificationAsync(new Notification { UserId = recipient, Title = "New project document", Message = $"{document.Title} was added to a project.", Type = NotificationType.ProjectDocumentAdded });
            }
            return new UploadResult(request.FileName, true, null, ToSummary(document));
        }
        catch (Exception ex) when (ex is IOException or DbUpdateException)
        {
            await CleanupAsync(quarantineKey, key);
            return Fail(request, "The upload could not be completed.");
        }
    }

    public Task<IReadOnlyList<DocumentSummary>> ListMyDocumentsAsync(int userId, DocumentQuery query) => Query(_context.Documents.Where(d => d.UploadedByUserId == userId), query);
    public Task<IReadOnlyList<DocumentSummary>> ListProjectDocumentsAsync(int userId, int projectId, DocumentQuery query) => Query(_authorization.AccessibleDocuments(userId).Where(d => d.ProjectId == projectId), query);
    public Task<IReadOnlyList<DocumentSummary>> ListSharedDocumentsAsync(int userId, DocumentQuery query) => Query(_authorization.AccessibleDocuments(userId).Where(d => d.Shares.Any(s => s.RevokedAt == null && (s.UserId == userId || (s.TeamId.HasValue && s.Team!.Memberships.Any(m => m.UserId == userId && m.IsActive))))), query);
    public Task<IReadOnlyList<DocumentSummary>> SearchAsync(int userId, DocumentQuery query) => Query(_authorization.AccessibleDocuments(userId), query);

    public async Task<DocumentSummary?> GetMetadataAsync(int userId, int documentId) => (await _authorization.AccessibleDocuments(userId).Include(d => d.UploadedByUser).FirstOrDefaultAsync(d => d.DocumentId == documentId)) is { } d ? ToSummary(d) : null;
    public async Task<(Stream Content, string ContentType, string FileName)?> OpenForRetrievalAsync(int userId, int documentId)
    {
        var document = await _authorization.AccessibleDocuments(userId).FirstOrDefaultAsync(d => d.DocumentId == documentId);
        if (document == null) return null;
        _context.DocumentActivities.Add(new DocumentActivity { DocumentId = document.DocumentId, DocumentIdentity = document.OriginalFileName, ActorUserId = userId, Action = "Download" });
        await _context.SaveChangesAsync();
        return (await _storage.OpenReadAsync(document.StoragePath), document.FileType, SafeDownloadName(document.OriginalFileName));
    }
    public async Task<bool> UpdateMetadataAsync(int userId, int documentId, string title, string category, string? description) { var d = await _context.Documents.FindAsync(documentId); if (d == null || !await _authorization.CanManageAsync(userId, d)) return false; d.Title = title.Trim(); d.Category = category.Trim(); d.Description = description; _context.DocumentActivities.Add(new DocumentActivity { DocumentId = d.DocumentId, DocumentIdentity = d.OriginalFileName, ActorUserId = userId, Action = "MetadataUpdate" }); await _context.SaveChangesAsync(); return true; }
    public async Task<bool> ReplaceFileAsync(int userId, int documentId, DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FindAsync(documentId);
        var validation = Validate(request);
        if (document == null || validation != null || !await _authorization.CanManageAsync(userId, document)) return false;
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var quarantineKey = Guid.NewGuid().ToString("N") + extension;
        try
        {
            await _storage.UploadAsync(request.Content, quarantineKey, request.ContentType, true, cancellationToken);
            await using var scanStream = await _storage.OpenReadAsync(quarantineKey, true, cancellationToken);
            if (await _scanner.ScanAsync(scanStream, request.ContentType, cancellationToken) != ScanResult.Clean)
            {
                await CleanupAsync(quarantineKey);
                return false;
            }
            await using var staged = await _storage.OpenReadAsync(quarantineKey, true, cancellationToken);
            await _storage.DeleteAsync(document.StoragePath, false, cancellationToken);
            var key = $"{userId}/{document.ProjectId?.ToString() ?? "personal"}/{Guid.NewGuid():N}{extension}";
            await _storage.UploadAsync(staged, key, request.ContentType, false, cancellationToken);
            document.StoragePath = key; document.OriginalFileName = Path.GetFileName(request.FileName); document.FileType = request.ContentType; document.FileSize = request.FileSize; document.ScanStatus = DocumentScanStatus.Replaced;
            _context.DocumentActivities.Add(new DocumentActivity { DocumentId = document.DocumentId, DocumentIdentity = document.OriginalFileName, ActorUserId = userId, Action = "Replace" });
            await _context.SaveChangesAsync(cancellationToken);
            await _storage.DeleteAsync(quarantineKey, true, cancellationToken);
            return true;
        }
        catch (IOException) { await CleanupAsync(quarantineKey); return false; }
    }
    public async Task<bool> DeleteAsync(int userId, int documentId) { var d = await _context.Documents.FindAsync(documentId); if (d == null || !await _authorization.CanManageAsync(userId, d)) return false; await _storage.DeleteAsync(d.StoragePath); d.ScanStatus = DocumentScanStatus.Deleted; _context.DocumentActivities.Add(new DocumentActivity { DocumentId = d.DocumentId, DocumentIdentity = d.OriginalFileName, ActorUserId = userId, Action = "Delete" }); await _context.SaveChangesAsync(); return true; }
    public async Task<bool> ShareAsync(int userId, int documentId, int? targetUserId, int? targetTeamId) { var d = await _context.Documents.FindAsync(documentId); if (d == null || !await _authorization.CanManageAsync(userId, d) || (targetUserId.HasValue == targetTeamId.HasValue)) return false; _context.DocumentShares.Add(new DocumentShare { DocumentId = documentId, UserId = targetUserId, TeamId = targetTeamId, GrantedByUserId = userId }); _context.DocumentActivities.Add(new DocumentActivity { DocumentId = d.DocumentId, DocumentIdentity = d.OriginalFileName, ActorUserId = userId, Action = "Share" }); await _context.SaveChangesAsync(); if (targetUserId.HasValue) await _notifications.CreateNotificationAsync(new Notification { UserId = targetUserId.Value, Title = "Document shared with you", Message = $"{d.Title} was shared with you.", Type = NotificationType.DocumentShared }); return true; }
    public async Task<bool> RevokeShareAsync(int userId, int documentId, int shareId) { var d = await _context.Documents.FindAsync(documentId); var share = await _context.DocumentShares.FirstOrDefaultAsync(s => s.DocumentShareId == shareId && s.DocumentId == documentId && s.RevokedAt == null); if (d == null || share == null || !await _authorization.CanManageAsync(userId, d)) return false; share.RevokedAt = DateTime.UtcNow; _context.DocumentActivities.Add(new DocumentActivity { DocumentId = d.DocumentId, DocumentIdentity = d.OriginalFileName, ActorUserId = userId, Action = "RevokeShare" }); await _context.SaveChangesAsync(); return true; }
    public Task<IReadOnlyList<DocumentSummary>> GetRecentUploadsAsync(int userId, int limit = 5) => Query(_authorization.AccessibleDocuments(userId), new DocumentQuery(Sort: "date"), limit);
    public Task<int> GetDocumentCountAsync(int userId) => _authorization.AccessibleDocuments(userId).CountAsync().ContinueWith(t => t.Result);
    public Task<UploadResult> AttachToTaskAsync(int userId, int taskId, DocumentUploadRequest request, CancellationToken cancellationToken = default) => UploadAsync(userId, request with { TaskId = taskId }, cancellationToken);

    private async Task<IReadOnlyList<DocumentSummary>> Query(IQueryable<Document> source, DocumentQuery query, int? limit = null)
    {
        if (!string.IsNullOrWhiteSpace(query.Search)) source = source.Where(d => d.Title.Contains(query.Search) || (d.Description != null && d.Description.Contains(query.Search)) || d.OriginalFileName.Contains(query.Search));
        if (!string.IsNullOrWhiteSpace(query.Category)) source = source.Where(d => d.Category == query.Category);
        if (query.ProjectId.HasValue) source = source.Where(d => d.ProjectId == query.ProjectId);
        if (query.From.HasValue) source = source.Where(d => d.UploadedAt >= query.From);
        if (query.To.HasValue) source = source.Where(d => d.UploadedAt <= query.To);
        source = query.Sort.ToLowerInvariant() switch { "title" => source.OrderBy(d => d.Title), "size" => source.OrderByDescending(d => d.FileSize), "category" => source.OrderBy(d => d.Category), _ => source.OrderByDescending(d => d.UploadedAt) };
        return await source.Include(d => d.UploadedByUser).Take(limit ?? 500).Select(d => new DocumentSummary(d.DocumentId, d.Title, d.Category, d.OriginalFileName, d.FileType, d.FileSize, d.UploadedAt, d.ProjectId, d.TaskId, d.UploadedByUser.DisplayName)).ToListAsync();
    }
    private async Task<bool> CanProjectAsync(int userId, int projectId) => await _context.Projects.AnyAsync(p => p.ProjectId == projectId && (p.ProjectManagerId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId) || _context.Users.Any(u => u.UserId == userId && u.Role == UserRole.Administrator)));
    private static string? Validate(DocumentUploadRequest r) => string.IsNullOrWhiteSpace(r.Title) ? "A title is required." : string.IsNullOrWhiteSpace(r.Category) ? "A category is required." : r.FileSize <= 0 || r.FileSize > DocumentRules.MaxFileSize ? "The file must be no larger than 25 MB." : !DocumentRules.IsAllowedContentType(r.ContentType) ? "This file type is not supported." : r.ContentType.Length > 255 ? "The file type is too long." : null;
    private static UploadResult Fail(DocumentUploadRequest r, string error) => new(r.FileName, false, error, null);
    private static DocumentSummary ToSummary(Document d) => new(d.DocumentId, d.Title, d.Category, d.OriginalFileName, d.FileType, d.FileSize, d.UploadedAt, d.ProjectId, d.TaskId, d.UploadedByUser?.DisplayName);
    private static string SafeDownloadName(string name) => Path.GetFileName(name).Replace("\"", "", StringComparison.Ordinal);
    private async Task CleanupAsync(params string[] keys) { foreach (var key in keys) { try { await _storage.DeleteAsync(key, key.Contains("/", StringComparison.Ordinal) == false); } catch { } } }
}
