using System.Collections.Concurrent;
using ClientReportingPortal.Web.Contracts.SharePoint;

namespace ClientReportingPortal.Web.Services.SharePoint;

/// <summary>
/// Demo/offline implementation of <see cref="ISharePointDocumentService"/>. Serves a realistic,
/// in-memory document library seeded with fake metadata - no Microsoft Graph, no Azure AD app
/// registration, no network call. Uploaded files are stored only as metadata + a byte count;
/// content itself is never persisted to disk.
///
/// --- What RealSharePointDocumentService would need ---
/// 1. SDK: <c>Microsoft.Graph</c> (GraphServiceClient) rather than hand-rolled HTTP calls.
/// 2. Auth: app-only (client-credentials) auth via <c>Microsoft.Identity.Client</c>
///    (ConfidentialClientApplication) or <c>Azure.Identity.ClientSecretCredential</c> - no signed-in
///    user is required for a background reporting portal reading a shared library.
/// 3. Scopes: least privilege - application permission <c>Sites.Read.All</c> for listing/reading,
///    <c>Sites.ReadWrite.All</c> only if upload is required, scoped further to a specific site via a
///    Graph application access policy where possible rather than tenant-wide.
/// 4. Resilience: Graph throttles aggressively (HTTP 429/503 with a Retry-After header) under load;
///    wrap calls in a Polly retry policy that honors Retry-After, per this framework's SharePoint/Graph
///    integration guidance (see wiki/integrations/sharepoint-graph.md conceptually - not reproduced here).
/// 5. Paging: <c>ListDocumentsAsync</c> would need to follow Graph's <c>@odata.nextLink</c> for large
///    libraries instead of returning a single page.
/// </summary>
public sealed class MockSharePointDocumentService : ISharePointDocumentService
{
    private readonly ConcurrentDictionary<string, SharePointDocument> _documents;

    public MockSharePointDocumentService()
    {
        var seed = new[]
        {
            new SharePointDocument
            {
                Id = "01ABCDEF000001",
                Name = "Q3-Financial-Summary.pdf",
                SizeInBytes = 482_331,
                LastModifiedUtc = DateTimeOffset.UtcNow.AddDays(-2),
                LastModifiedBy = "Priya Sharma",
                WebUrl = "https://contoso.sharepoint.com/sites/ClientReporting/Shared%20Documents/Q3-Financial-Summary.pdf",
                ContentType = "application/pdf",
            },
            new SharePointDocument
            {
                Id = "01ABCDEF000002",
                Name = "Master-Service-Agreement.docx",
                SizeInBytes = 118_204,
                LastModifiedUtc = DateTimeOffset.UtcNow.AddDays(-14),
                LastModifiedBy = "Arvind Kushwaha",
                WebUrl = "https://contoso.sharepoint.com/sites/ClientReporting/Shared%20Documents/Master-Service-Agreement.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            },
            new SharePointDocument
            {
                Id = "01ABCDEF000003",
                Name = "Onboarding-Checklist.xlsx",
                SizeInBytes = 54_998,
                LastModifiedUtc = DateTimeOffset.UtcNow.AddHours(-6),
                LastModifiedBy = "Priya Sharma",
                WebUrl = "https://contoso.sharepoint.com/sites/ClientReporting/Shared%20Documents/Onboarding-Checklist.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            },
            new SharePointDocument
            {
                Id = "01ABCDEF000004",
                Name = "Architecture-Overview.pptx",
                SizeInBytes = 2_114_050,
                LastModifiedUtc = DateTimeOffset.UtcNow.AddDays(-30),
                LastModifiedBy = "System Administrator",
                WebUrl = "https://contoso.sharepoint.com/sites/ClientReporting/Shared%20Documents/Architecture-Overview.pptx",
                ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            },
        };

        _documents = new ConcurrentDictionary<string, SharePointDocument>(
            seed.ToDictionary(d => d.Id, d => d));
    }

    public Task<IReadOnlyList<SharePointDocument>> ListDocumentsAsync(
        string siteId,
        string driveId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(siteId);
        ArgumentException.ThrowIfNullOrWhiteSpace(driveId);

        IReadOnlyList<SharePointDocument> result = _documents.Values
            .OrderByDescending(d => d.LastModifiedUtc)
            .ToList();

        return Task.FromResult(result);
    }

    public async Task<SharePointDocument> UploadDocumentAsync(
        string siteId,
        string driveId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(siteId);
        ArgumentException.ThrowIfNullOrWhiteSpace(driveId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        // The mock only needs the length, mirroring how a real upload would report the
        // resulting DriveItem's size without the caller needing to compute it up front.
        long length;
        if (content.CanSeek)
        {
            length = content.Length;
        }
        else
        {
            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            length = buffer.Length;
        }

        var document = new SharePointDocument
        {
            Id = $"01MOCK{Guid.NewGuid():N}"[..14],
            Name = fileName,
            SizeInBytes = length,
            LastModifiedUtc = DateTimeOffset.UtcNow,
            LastModifiedBy = "Demo User",
            WebUrl = $"https://contoso.sharepoint.com/sites/ClientReporting/Shared%20Documents/{Uri.EscapeDataString(fileName)}",
            ContentType = "application/octet-stream",
        };

        _documents[document.Id] = document;
        return document;
    }

    public Task<string> GetDocumentDownloadUrlAsync(
        string siteId,
        string driveId,
        string itemId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(siteId);
        ArgumentException.ThrowIfNullOrWhiteSpace(driveId);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        if (!_documents.TryGetValue(itemId, out var document))
        {
            throw new KeyNotFoundException($"No document with id '{itemId}' in this mock library.");
        }

        // Real Graph responses hand back a pre-signed, time-limited Azure Blob Storage URL via
        // the "@microsoft.graph.downloadUrl" facet - this mock just returns the item's web URL.
        return Task.FromResult(document.WebUrl);
    }
}
