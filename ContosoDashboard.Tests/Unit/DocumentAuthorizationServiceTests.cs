using ContosoDashboard.Models;
using ContosoDashboard.Services;

namespace ContosoDashboard.Tests.Unit;

public class DocumentAuthorizationServiceTests
{
    [Fact]
    public void Share_requires_exactly_one_target()
    {
        Assert.True(new DocumentShare { UserId = 4 }.HasExactlyOneTarget);
        Assert.True(new DocumentShare { TeamId = 2 }.HasExactlyOneTarget);
        Assert.False(new DocumentShare().HasExactlyOneTarget);
        Assert.False(new DocumentShare { UserId = 4, TeamId = 2 }.HasExactlyOneTarget);
    }

    [Fact]
    public async Task Scanner_rejects_threat_and_failure_markers()
    {
        var scanner = new DeterministicMalwareScanner();
        Assert.Equal(ScanResult.Threat, await scanner.ScanAsync(new MemoryStream("EICAR"u8.ToArray()), "text/plain"));
        Assert.Equal(ScanResult.Failed, await scanner.ScanAsync(new MemoryStream("SCAN_FAILURE"u8.ToArray()), "text/plain"));
        Assert.Equal(ScanResult.Clean, await scanner.ScanAsync(new MemoryStream("clean"u8.ToArray()), "text/plain"));
    }
}
