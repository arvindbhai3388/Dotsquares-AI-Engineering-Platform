namespace ClientReportingPortal.Web.Contracts.PowerBi;

/// <summary>
/// Contract for obtaining a Power BI embed configuration (embed URL + short-lived embed token)
/// for a single report, matching the shape of the real Power BI REST "GenerateToken" flow.
///
/// This is the seam this demo teaches: the dashboard controller and Razor view only ever talk
/// to <see cref="IPowerBiEmbedService"/>. Swapping the DI registration in Program.cs from
/// <see cref="Services.PowerBi.MockPowerBiEmbedService"/> to a real implementation is the
/// entire migration - no controller/view changes required.
/// </summary>
public interface IPowerBiEmbedService
{
    /// <summary>
    /// Produces an <see cref="EmbedConfig"/> for the given report/workspace, scoped to
    /// <paramref name="effectiveIdentity"/> for row-level security.
    /// </summary>
    /// <param name="reportId">Power BI report GUID.</param>
    /// <param name="workspaceId">Power BI workspace (group) GUID the report belongs to.</param>
    /// <param name="effectiveIdentity">
    /// RLS identity to embed in the token so the report only returns rows the current portal user may see.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<EmbedConfig> GetEmbedTokenAsync(
        string reportId,
        string workspaceId,
        EffectiveIdentity effectiveIdentity,
        CancellationToken cancellationToken = default);
}
