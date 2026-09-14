using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using ContosoDashboard.Services;

namespace ContosoDashboard.Pages;

public class DocumentDownloadModel : PageModel
{
    private readonly IDocumentService _documents;
    public DocumentDownloadModel(IDocumentService documents) => _documents = documents;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return NotFound();
        var result = await _documents.OpenForRetrievalAsync(userId, id);
        if (result is null) return NotFound();
        var response = new FileStreamResult(result.Value.Content, result.Value.ContentType) { FileDownloadName = result.Value.FileName, EnableRangeProcessing = true };
        return response;
    }
}
