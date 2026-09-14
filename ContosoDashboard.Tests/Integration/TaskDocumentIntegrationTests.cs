using ContosoDashboard.Tests.Fixtures;

namespace ContosoDashboard.Tests.Integration;

public class TaskDocumentIntegrationTests
{
    [Fact]
    public async Task Authorized_task_attachment_is_visible_and_mismatched_project_is_rejected()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var service = DocumentTestData.Service(context, new InMemoryDocumentStorage(), new ConfigurableScanner());

        var attached = await service.AttachToTaskAsync(4, 2, DocumentTestData.Request(projectId: 1));
        var denied = await service.AttachToTaskAsync(3, 2, DocumentTestData.Request(projectId: 999));

        Assert.True(attached.Success);
        Assert.Equal(2, attached.Document!.TaskId);
        Assert.Single(context.Tasks.Where(t => t.TaskId == 2).SelectMany(t => t.Documents));
        Assert.False(denied.Success);
    }
}