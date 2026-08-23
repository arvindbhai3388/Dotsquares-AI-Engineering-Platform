namespace ClientReportingPortal.Web.Contracts.SharePoint;

/// <summary>
/// Contract for listing, uploading, and generating download links for documents stored in a
/// SharePoint document library, matching the shape of real Microsoft Graph SDK calls
/// (<c>GraphServiceClient.Sites[siteId].Drives[driveId].Items...</c>).
///
/// This is the seam this demo teaches: the documents controller/view only ever talk to
/// <see cref="ISharePointDocumentService"/>. Swapping the DI registration in Program.cs from
/// <see cref="Services.SharePoint.MockSharePointDocumentService"/> to a real Graph-backed
/// implementation is the entire migration - no controller/view changes required.
/// </summary>
public interface ISharePointDocumentService
{
    /// <summary>
    /// Lists the documents in a drive. Real shape: Graph
    /// <c>GET /sites/{siteId}/drives/{driveId}/root/children</c>.
    /// </summary>
    Task<IReadOnlyList<SharePointDocument>> ListDocumentsAsync(
        string siteId,
        string driveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a document's content. Real shape: Graph
    /// <c>PUT /sites/{siteId}/drives/{driveId}/root:/{fileName}:/content</c> (small files) or a Graph
    /// upload session for anything over ~4 MB.
    /// </summary>
    Task<SharePointDocument> UploadDocumentAsync(
        string siteId,
        string driveId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Produces a short-lived, direct download URL for a document. Real shape: Graph
    /// <c>GET /sites/{siteId}/drives/{driveId}/items/{itemId}</c>, reading
    /// <c>@microsoft.graph.downloadUrl</c> from the response.
    /// </summary>
    Task<string> GetDocumentDownloadUrlAsync(
        string siteId,
        string driveId,
        string itemId,
        CancellationToken cancellationToken = default);
}
