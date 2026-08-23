using ClientReportingPortal.Web.Contracts.PowerBi;
using ClientReportingPortal.Web.Services.PowerBi;

namespace ClientReportingPortal.Tests.Services;

public class MockPowerBiEmbedServiceTests
{
    private readonly MockPowerBiEmbedService _sut = new();

    [Fact]
    public async Task GetEmbedTokenAsync_ReturnsConfigEchoingReportAndWorkspaceIds()
    {
        var identity = new EffectiveIdentity("alice@contoso.example", new[] { "ClientReader" }, new[] { "ds-1" });

        var result = await _sut.GetEmbedTokenAsync("report-123", "workspace-456", identity);

        Assert.Equal("report-123", result.ReportId);
        Assert.Equal("workspace-456", result.WorkspaceId);
        Assert.Equal("report", result.EmbedType);
    }

    [Fact]
    public async Task GetEmbedTokenAsync_EmbedUrlContainsReportAndWorkspaceIds()
    {
        var identity = new EffectiveIdentity("alice@contoso.example", new[] { "ClientReader" }, new[] { "ds-1" });

        var result = await _sut.GetEmbedTokenAsync("report-123", "workspace-456", identity);

        Assert.Contains("report-123", result.EmbedUrl);
        Assert.Contains("workspace-456", result.EmbedUrl);
    }

    [Fact]
    public async Task GetEmbedTokenAsync_ReturnsTokenThatExpiresInTheFuture()
    {
        var identity = new EffectiveIdentity("alice@contoso.example", new[] { "ClientReader" }, new[] { "ds-1" });
        var before = DateTimeOffset.UtcNow;

        var result = await _sut.GetEmbedTokenAsync("report-123", "workspace-456", identity);

        Assert.False(string.IsNullOrWhiteSpace(result.EmbedToken));
        Assert.True(result.TokenExpiresUtc > before);
    }

    [Theory]
    [InlineData(null, "workspace-456")]
    [InlineData("", "workspace-456")]
    [InlineData("report-123", null)]
    [InlineData("report-123", "")]
    public async Task GetEmbedTokenAsync_ThrowsForMissingReportOrWorkspaceId(string? reportId, string? workspaceId)
    {
        var identity = new EffectiveIdentity("alice@contoso.example", new[] { "ClientReader" }, new[] { "ds-1" });

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => _sut.GetEmbedTokenAsync(reportId!, workspaceId!, identity));
    }

    [Fact]
    public async Task GetEmbedTokenAsync_ThrowsForNullEffectiveIdentity()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.GetEmbedTokenAsync("report-123", "workspace-456", null!));
    }
}
