namespace ClientReportingPortal.Web.Contracts.PowerBi;

/// <summary>
/// Everything the browser-side "powerbi-client" SDK needs to render an embedded report.
/// Shape mirrors the payload a real Power BI "GenerateToken" REST call would produce,
/// so swapping <see cref="Services.PowerBi.MockPowerBiEmbedService"/> for a real
/// implementation requires no changes to the controller or the view.
/// </summary>
public sealed class EmbedConfig
{
    /// <summary>The Power BI report GUID being embedded.</summary>
    public required string ReportId { get; init; }

    /// <summary>The workspace (group) GUID the report lives in.</summary>
    public required string WorkspaceId { get; init; }

    /// <summary>
    /// The iframe embed URL, e.g. "https://app.powerbi.com/reportEmbed?reportId={id}&amp;groupId={groupId}".
    /// </summary>
    public required string EmbedUrl { get; init; }

    /// <summary>
    /// Short-lived embed token. In a real implementation this comes back from Power BI's
    /// GenerateToken API and is scoped to the effective identity passed in the request.
    /// Never a user's own AAD token - always report-scoped.
    /// </summary>
    public required string EmbedToken { get; init; }

    /// <summary>UTC instant the embed token stops being valid (Power BI tokens are short-lived, ~1 hour).</summary>
    public DateTimeOffset TokenExpiresUtc { get; init; }

    /// <summary>"report", "dashboard", or "tile" - matches the powerbi-client "tokenType"/"embedType" options.</summary>
    public string EmbedType { get; init; } = "report";
}
