namespace ClientReportingPortal.Web.Contracts.SharePoint;

/// <summary>
/// Document metadata shape, deliberately mirroring the subset of fields a Microsoft Graph
/// <c>DriveItem</c> exposes (id, name, size, lastModifiedDateTime, webUrl, file.mimeType),
/// so a real implementation can map 1:1 from the Graph SDK model onto this DTO.
/// </summary>
public sealed class SharePointDocument
{
    /// <summary>Graph drive-item id.</summary>
    public required string Id { get; init; }

    public required string Name { get; init; }

    public long SizeInBytes { get; init; }

    public DateTimeOffset LastModifiedUtc { get; init; }

    /// <summary>Display name of the user who last modified the item (Graph: lastModifiedBy.user.displayName).</summary>
    public required string LastModifiedBy { get; init; }

    /// <summary>Browser-viewable SharePoint URL (Graph: webUrl).</summary>
    public required string WebUrl { get; init; }

    /// <summary>MIME type (Graph: file.mimeType).</summary>
    public required string ContentType { get; init; }
}
