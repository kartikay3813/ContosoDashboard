using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public sealed record DocumentStorageOptions(string RootPath, string QuarantinePath);

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string storageKey, string contentType, bool quarantine = false, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default);
}

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly DocumentStorageOptions _options;
    public LocalFileStorageService(DocumentStorageOptions options, IHostEnvironment environment)
    {
        _options = new DocumentStorageOptions(
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.RootPath)),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.QuarantinePath)));
    }

    public async Task<string> UploadAsync(Stream content, string storageKey, string contentType, bool quarantine = false, CancellationToken cancellationToken = default)
    {
        var fullPath = Resolve(storageKey, quarantine);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(output, cancellationToken);
        return storageKey;
    }

    public Task DeleteAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default)
    {
        var path = Resolve(storageKey, quarantine);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Stream>(new FileStream(Resolve(storageKey, quarantine), FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task<bool> ExistsAsync(string storageKey, bool quarantine = false, CancellationToken cancellationToken = default)
        => Task.FromResult(File.Exists(Resolve(storageKey, quarantine)));

    private string Resolve(string storageKey, bool quarantine)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.IsPathRooted(storageKey) || storageKey.Split('/', '\\').Any(segment => segment is ".." or ""))
            throw new ArgumentException("Storage keys must be relative and safe.", nameof(storageKey));
        var root = quarantine ? _options.QuarantinePath : _options.RootPath;
        var fullPath = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("Storage key escapes the configured root.", nameof(storageKey));
        return fullPath;
    }
}
