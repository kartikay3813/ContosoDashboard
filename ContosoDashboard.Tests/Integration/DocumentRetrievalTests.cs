using System.Security.Claims;
using ContosoDashboard.Models;
using ContosoDashboard.Pages;
using ContosoDashboard.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoDashboard.Tests.Integration;

public class DocumentRetrievalTests
{
    [Fact]
    public async Task Authorized_retrieval_returns_content_type_and_safe_filename()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var storage = new InMemoryDocumentStorage();
        var service = DocumentTestData.Service(context, storage, new ConfigurableScanner());
        var upload = await service.UploadAsync(4, DocumentTestData.Request(name: "../../download.pdf"));
        var page = new DocumentDownloadModel(service) { PageContext = PageContextFor(4) };

        var result = await page.OnGetAsync(upload.Document!.DocumentId);

        var file = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("download.pdf", file.FileDownloadName);
    }

    [Fact]
    public async Task Missing_and_unauthorized_retrieval_are_both_not_found()
    {
        var (context, connection) = TestDataFactory.CreateContext();
        await using var _ = connection;
        var service = DocumentTestData.Service(context, new InMemoryDocumentStorage(), new ConfigurableScanner());
        var upload = await service.UploadAsync(4, DocumentTestData.Request());

        var unauthorized = new DocumentDownloadModel(service) { PageContext = PageContextFor(3) };
        var missing = new DocumentDownloadModel(service) { PageContext = PageContextFor(3) };
        Assert.IsType<NotFoundResult>(await unauthorized.OnGetAsync(upload.Document!.DocumentId));
        Assert.IsType<NotFoundResult>(await missing.OnGetAsync(99999));
    }

    private static PageContext PageContextFor(int userId)
    {
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test")) };
        return new PageContext { HttpContext = context };
    }
}