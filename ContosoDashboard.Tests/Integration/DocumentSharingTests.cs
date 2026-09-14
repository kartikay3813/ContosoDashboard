using ContosoDashboard.Models;
using ContosoDashboard.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Tests.Integration;

public class DocumentSharingTests
{
    [Fact]
    public async Task Direct_share_survives_project_membership_loss_and_revoke_removes_access()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var storage = new InMemoryDocumentStorage();
        var service = DocumentTestData.Service(context, storage, new ConfigurableScanner());
        var upload = await service.UploadAsync(4, DocumentTestData.Request(projectId: 1));
        var id = upload.Document!.DocumentId;
        Assert.True(await service.ShareAsync(4, id, 3, null));
        var share = await context.DocumentShares.SingleAsync(s => s.DocumentId == id);
        context.ProjectMembers.RemoveRange(context.ProjectMembers.Where(m => m.ProjectId == 1 && m.UserId == 3));
        await context.SaveChangesAsync();

        Assert.NotNull(await service.GetMetadataAsync(3, id));
        Assert.True(await service.RevokeShareAsync(4, id, share.DocumentShareId));
        Assert.Null(await service.GetMetadataAsync(3, id));
        Assert.Contains(context.Notifications, n => n.UserId == 3 && n.Type == NotificationType.DocumentShared);
    }

    [Fact]
    public async Task Team_share_follows_active_membership()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        context.Teams.Add(new Team { TeamId = 20, Name = "Design" });
        context.TeamMemberships.Add(new TeamMembership { TeamId = 20, UserId = 3, IsActive = true });
        await context.SaveChangesAsync();
        var service = DocumentTestData.Service(context, new InMemoryDocumentStorage(), new ConfigurableScanner());
        var upload = await service.UploadAsync(4, DocumentTestData.Request());

        Assert.True(await service.ShareAsync(4, upload.Document!.DocumentId, null, 20));
        Assert.NotNull(await service.GetMetadataAsync(3, upload.Document.DocumentId));
        context.TeamMemberships.Single(m => m.TeamId == 20 && m.UserId == 3).IsActive = false;
        await context.SaveChangesAsync();
        Assert.Null(await service.GetMetadataAsync(3, upload.Document.DocumentId));
    }
}