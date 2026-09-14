using ContosoDashboard.Data;
using ContosoDashboard.Services;
using System.Text;

namespace ContosoDashboard.Tests.Integration;

internal sealed class InMemoryDocumentStorage : IFileStorageService
{
    private readonly Dictionary<(bool Quarantine, string Key), byte[]> files = new();
    public bool FailPermanentUploads { get; set; }
    public IReadOnlyCollection<string> Keys => files.Keys.Select(k => k.Key).ToArray();

    public async Task<string> UploadAsync(Stream content, string storageKey, string contentType, bool quarantine = false, CancellationToken cancellationToken = default)
    {
        if (FailPermanentUploads && !quarantine) throw new IOException("configured storage failure");
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        files[(quarantine, storageKey)] = buffer.ToArray();
        return storageKey;
    }

    public Task DeleteAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default)
    {
        files.Remove((quarantine, storageKey));
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(new MemoryStream(files[(quarantine, storageKey)]));

    public Task<bool> ExistsAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default)
        => Task.FromResult(files.ContainsKey((quarantine, storageKey)));
}

internal sealed class ConfigurableScanner : IMalwareScanner
{
    public ScanResult Result { get; set; } = ScanResult.Clean;
    public Task<ScanResult> ScanAsync(Stream quarantinedContent, string detectedContentType, CancellationToken cancellationToken = default)
        => Task.FromResult(Result);
}

internal static class DocumentTestData
{
    public static DocumentUploadRequest Request(string name = "report.pdf", string title = "Report", int? projectId = null, int? taskId = null, string content = "clean")
        => new(title, "Reports", name, "application/pdf", Encoding.UTF8.GetByteCount(content), new MemoryStream(Encoding.UTF8.GetBytes(content)), ProjectId: projectId, TaskId: taskId);

    public static DocumentService Service(ApplicationDbContext context, InMemoryDocumentStorage storage, ConfigurableScanner scanner)
        => new(context, storage, scanner, new DocumentAuthorizationService(context), new NotificationService(context));
}