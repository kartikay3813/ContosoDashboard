using ContosoDashboard.Models;
using ContosoDashboard.Services;

namespace ContosoDashboard.Tests.Unit;

public class DocumentUploadValidationTests
{
    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("image/jpeg", true)]
    [InlineData("application/x-msdownload", false)]
    public void Allowlist_accepts_supported_types_only(string contentType, bool expected)
        => Assert.Equal(expected, DocumentRules.IsAllowedContentType(contentType));

    [Fact]
    public void Enforces_exact_25_mb_boundary()
        => Assert.Equal(25 * 1024 * 1024, DocumentRules.MaxFileSize);
}
