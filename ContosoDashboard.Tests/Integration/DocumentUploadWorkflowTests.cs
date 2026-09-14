using ContosoDashboard.Tests.Fixtures;
using ContosoDashboard.Services;

namespace ContosoDashboard.Tests.Integration;

public class DocumentUploadWorkflowTests
{
    [Fact]
    public async Task Clean_upload_persists_document_and_removes_quarantine_file()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var storage = new InMemoryDocumentStorage();
        var service = DocumentTestData.Service(context, storage, new ConfigurableScanner());

        var result = await service.UploadAsync(4, DocumentTestData.Request(projectId: 1));

        Assert.True(result.Success);
        Assert.Single(context.Documents);
        Assert.DoesNotContain(storage.Keys, key => key.Length == 32);
    }

    [Theory]
    [InlineData(ScanResult.Threat)]
    [InlineData(ScanResult.Failed)]
    public async Task Scanner_rejection_does_not_create_metadata_or_permanent_file(ScanResult scanResult)
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var storage = new InMemoryDocumentStorage();
        var scanner = new ConfigurableScanner { Result = scanResult };
        var result = await DocumentTestData.Service(context, storage, scanner).UploadAsync(4, DocumentTestData.Request());

        Assert.False(result.Success);
        Assert.Empty(context.Documents);
        Assert.Empty(storage.Keys);
    }

    [Fact]
    public async Task Storage_failure_cleans_quarantine_and_leaves_no_orphan_record()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var storage = new InMemoryDocumentStorage { FailPermanentUploads = true };

        var result = await DocumentTestData.Service(context, storage, new ConfigurableScanner()).UploadAsync(4, DocumentTestData.Request());

        Assert.False(result.Success);
        Assert.Empty(context.Documents);
        Assert.Empty(storage.Keys);
    }
}