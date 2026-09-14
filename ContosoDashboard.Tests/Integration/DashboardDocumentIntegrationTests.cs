using ContosoDashboard.Models;
using ContosoDashboard.Services;
using ContosoDashboard.Tests.Fixtures;

namespace ContosoDashboard.Tests.Integration;

public class DashboardDocumentIntegrationTests
{
    [Fact]
    public async Task Dashboard_reports_document_count_recent_five_and_notifications()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var storage = new InMemoryDocumentStorage();
        var documents = DocumentTestData.Service(context, storage, new ConfigurableScanner());
        for (var index = 0; index < 6; index++) await documents.UploadAsync(4, DocumentTestData.Request($"doc-{index}.pdf", $"Document {index}"));
        await documents.UploadAsync(4, DocumentTestData.Request("project.pdf", "Project document", 1));
        var dashboard = new DashboardService(context, documents);

        var summary = await dashboard.GetDashboardSummaryAsync(4);
        var recent = await dashboard.GetRecentDocumentsAsync(4);

        Assert.Equal(7, summary.DocumentCount);
        Assert.Equal(5, recent.Count);
        Assert.Contains(context.Notifications, n => n.Type == NotificationType.ProjectDocumentAdded);
    }
}