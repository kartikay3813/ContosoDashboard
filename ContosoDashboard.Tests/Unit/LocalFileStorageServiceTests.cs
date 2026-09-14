using ContosoDashboard.Services;

namespace ContosoDashboard.Tests.Unit;

public class LocalFileStorageServiceTests
{
    [Fact]
    public async Task Rejects_path_traversal_and_writes_inside_configured_root()
    {
        var root = Path.Combine(Path.GetTempPath(), "contoso-tests", Guid.NewGuid().ToString("N"));
        var environment = new TestHostEnvironment(root);
        var service = new LocalFileStorageService(new DocumentStorageOptions("documents", "quarantine"), environment);
        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadAsync(new MemoryStream([1]), "../escape.txt", "text/plain", true));
        await service.UploadAsync(new MemoryStream([1, 2]), "1/personal/file.txt", "text/plain");
        Assert.True(await service.ExistsAsync("1/personal/file.txt"));
        Directory.Delete(root, true);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string root) => ContentRootPath = root;
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; }
        public string WebRootPath { get; set; } = string.Empty;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
    }
}
