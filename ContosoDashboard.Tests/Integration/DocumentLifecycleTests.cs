using ContosoDashboard.Models;
using ContosoDashboard.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Tests.Integration;

public class DocumentLifecycleTests
{
    [Fact]
    public async Task Owner_can_update_replace_and_delete_document_while_unrelated_user_is_denied()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var storage = new InMemoryDocumentStorage();
        var service = DocumentTestData.Service(context, storage, new ConfigurableScanner());
        var upload = await service.UploadAsync(4, DocumentTestData.Request(name: "unsafe name.pdf"));
        var id = upload.Document!.DocumentId;

        Assert.False(await service.UpdateMetadataAsync(3, id, "No", "Reports", null));
        Assert.True(await service.UpdateMetadataAsync(4, id, "Updated", "Reports", "Description"));
        Assert.True(await service.ReplaceFileAsync(4, id, DocumentTestData.Request(name: "new.pdf", content: "replacement")));
        Assert.True(await service.DeleteAsync(4, id));
        Assert.Null(await service.GetMetadataAsync(4, id));
        Assert.Equal(DocumentScanStatus.Deleted, await context.Documents.Where(d => d.DocumentId == id).Select(d => d.ScanStatus).SingleAsync());
    }
}