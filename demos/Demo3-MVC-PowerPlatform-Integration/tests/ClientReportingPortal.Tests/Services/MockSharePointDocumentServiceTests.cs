using System.Text;
using ClientReportingPortal.Web.Services.SharePoint;

namespace ClientReportingPortal.Tests.Services;

public class MockSharePointDocumentServiceTests
{
    private readonly MockSharePointDocumentService _sut = new();

    [Fact]
    public async Task ListDocumentsAsync_ReturnsSeededDocumentsOrderedByMostRecentlyModified()
    {
        var documents = await _sut.ListDocumentsAsync("site-1", "drive-1");

        Assert.NotEmpty(documents);
        Assert.True(documents.SequenceEqual(documents.OrderByDescending(d => d.LastModifiedUtc)));
    }

    [Fact]
    public async Task UploadDocumentAsync_AddsDocumentThatThenAppearsInListing()
    {
        var content = Encoding.UTF8.GetBytes("hello world");
        using var stream = new MemoryStream(content);

        var uploaded = await _sut.UploadDocumentAsync("site-1", "drive-1", "notes.txt", stream);

        Assert.Equal("notes.txt", uploaded.Name);
        Assert.Equal(content.Length, uploaded.SizeInBytes);

        var documents = await _sut.ListDocumentsAsync("site-1", "drive-1");
        Assert.Contains(documents, d => d.Id == uploaded.Id);
    }

    [Fact]
    public async Task GetDocumentDownloadUrlAsync_ReturnsUrlForKnownDocument()
    {
        var documents = await _sut.ListDocumentsAsync("site-1", "drive-1");
        var existing = documents.First();

        var url = await _sut.GetDocumentDownloadUrlAsync("site-1", "drive-1", existing.Id);

        Assert.Equal(existing.WebUrl, url);
    }

    [Fact]
    public async Task GetDocumentDownloadUrlAsync_ThrowsForUnknownDocument()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetDocumentDownloadUrlAsync("site-1", "drive-1", "does-not-exist"));
    }

    [Theory]
    [InlineData(null, "drive-1")]
    [InlineData("", "drive-1")]
    [InlineData("site-1", null)]
    [InlineData("site-1", "")]
    public async Task ListDocumentsAsync_ThrowsForMissingSiteOrDriveId(string? siteId, string? driveId)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.ListDocumentsAsync(siteId!, driveId!));
    }
}
