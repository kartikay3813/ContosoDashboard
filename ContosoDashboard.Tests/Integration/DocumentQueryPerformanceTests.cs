using System.Diagnostics;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using ContosoDashboard.Tests.Fixtures;

namespace ContosoDashboard.Tests.Integration;

public class DocumentQueryPerformanceTests
{
    [Fact]
    public async Task Search_of_500_accessible_documents_completes_within_two_seconds()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        context.Documents.AddRange(Enumerable.Range(1, 500).Select(i => new Document { Title = $"Document {i}", Category = "Reports", OriginalFileName = $"document-{i}.pdf", StoragePath = $"perf-{i}", FileType = "application/pdf", FileSize = i, UploadedByUserId = 4, ScanStatus = DocumentScanStatus.Clean }));
        await context.SaveChangesAsync();
        var service = DocumentTestData.Service(context, new InMemoryDocumentStorage(), new ConfigurableScanner());
        var timer = Stopwatch.StartNew();

        var results = await service.SearchAsync(4, new DocumentQuery(Search: "Document"));

        timer.Stop();
        Assert.Equal(500, results.Count);
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(2), $"Query took {timer.Elapsed}.");
    }
}