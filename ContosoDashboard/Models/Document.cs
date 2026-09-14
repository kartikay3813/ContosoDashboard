using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public enum DocumentScanStatus { Quarantined, Clean, Rejected, Replaced, Deleted }

public static class DocumentRules
{
    public const long MaxFileSize = 25 * 1024 * 1024;
    public static readonly IReadOnlySet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/jpeg", "image/png", "text/plain", "text/csv",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    };

    public static bool IsAllowedContentType(string value) => AllowedContentTypes.Contains(value);
}

public class Document
{
    [Key] public int DocumentId { get; set; }
    [Required, MaxLength(255)] public string Title { get; set; } = string.Empty;
    [MaxLength(4000)] public string? Description { get; set; }
    [Required, MaxLength(50)] public string Category { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string OriginalFileName { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string StoragePath { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string FileType { get; set; } = string.Empty;
    [Range(1, DocumentRules.MaxFileSize)] public long FileSize { get; set; }
    [Required] public int UploadedByUserId { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    [Required] public DocumentScanStatus ScanStatus { get; set; } = DocumentScanStatus.Quarantined;
    public virtual User UploadedByUser { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual TaskItem? Task { get; set; }
    public virtual ICollection<DocumentTag> Tags { get; set; } = new List<DocumentTag>();
    public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
}
