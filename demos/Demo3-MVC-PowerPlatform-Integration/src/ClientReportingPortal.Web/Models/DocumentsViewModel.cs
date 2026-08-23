using ClientReportingPortal.Web.Contracts.SharePoint;

namespace ClientReportingPortal.Web.Models;

/// <summary>View model for the SharePoint/Graph documents page.</summary>
public sealed class DocumentsViewModel
{
    public required IReadOnlyList<SharePointDocument> Documents { get; init; }

    public string? StatusMessage { get; init; }
}
