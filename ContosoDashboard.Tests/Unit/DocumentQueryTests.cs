using ContosoDashboard.Models;
using ContosoDashboard.Services;
using ContosoDashboard.Tests.Integration;
using ContosoDashboard.Tests.Fixtures;

namespace ContosoDashboard.Tests.Unit;

public class DocumentQueryTests
{
    [Fact]
    public async Task Queries_apply_access_shares_filters_and_sorting()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        context.Documents.AddRange(
            new Document { Title = "Owned", Category = "Reports", OriginalFileName = "owned.pdf", StoragePath = "owned", FileType = "application/pdf", FileSize = 10, UploadedByUserId = 4, ScanStatus = DocumentScanStatus.Clean },
            new Document { Title = "Shared", Category = "Project Documents", OriginalFileName = "shared.pdf", StoragePath = "shared", FileType = "application/pdf", FileSize = 20, UploadedByUserId = 2, ProjectId = 1, ScanStatus = DocumentScanStatus.Clean },
            new Document { Title = "Hidden", Category = "Other", OriginalFileName = "hidden.pdf", StoragePath = "hidden", FileType = "application/pdf", FileSize = 30, UploadedByUserId = 3, ScanStatus = DocumentScanStatus.Clean });
        await context.SaveChangesAsync();
        var service = DocumentTestData.Service(context, new InMemoryDocumentStorage(), new ConfigurableScanner());

        var results = await service.SearchAsync(4, new DocumentQuery(Search: "Shared", Category: "Project Documents", Sort: "title"));

        var item = Assert.Single(results);
        Assert.Equal("Shared", item.Title);
        Assert.DoesNotContain(results, d => d.Title == "Hidden");
    }
}