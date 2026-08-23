using ClientReportingPortal.Web.Contracts.PowerBi;

namespace ClientReportingPortal.Web.Services.PowerBi;

/// <summary>
/// Demo/offline implementation of <see cref="IPowerBiEmbedService"/>. Returns a realistic-looking
/// but entirely fake <see cref="EmbedConfig"/> - no Azure AD, no Power BI tenant, no network call.
///
/// The embed token below is not a real JWT and will not authenticate against Power BI; it exists
/// purely so the Razor view's "powerbi-client" JS snippet has something to bind to and the wiring
/// is visibly end-to-end.
///
/// --- What RealPowerBiEmbedService would do differently ---
/// 1. Auth: use <c>Azure.Identity.ClientSecretCredential</c> (service-principal auth, "app owns data"
///    embedding pattern) to acquire an AAD token for the Power BI Service resource
///    (https://analysis.windows.net/powerbi/api), instead of a delegated user token.
/// 2. API call: use <c>Microsoft.PowerBI.Api</c>'s <c>PowerBIClient</c> (or a raw HTTP POST) against
///    POST https://api.powerbi.com/v1.0/myorg/groups/{workspaceId}/reports/{reportId}/GenerateToken
///    with an <c>IdentityBlob</c> containing <paramref name="effectiveIdentity"/> so Power BI applies
///    row-level security for the current portal user.
/// 3. Response: map the API's { token, expiration } onto <see cref="EmbedConfig"/>, plus the report's
///    own <c>embedUrl</c> from GET .../reports/{reportId} (do not hand-construct it as this mock does).
/// 4. Caching: embed tokens are short-lived (~1 hour); a real implementation should cache per
///    (reportId, effectiveIdentity) and refresh proactively before expiry, not on every page load.
/// </summary>
public sealed class MockPowerBiEmbedService : IPowerBiEmbedService
{
    public Task<EmbedConfig> GetEmbedTokenAsync(
        string reportId,
        string workspaceId,
        EffectiveIdentity effectiveIdentity,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceId);
        ArgumentNullException.ThrowIfNull(effectiveIdentity);

        var config = new EmbedConfig
        {
            ReportId = reportId,
            WorkspaceId = workspaceId,
            EmbedUrl = $"https://app.powerbi.com/reportEmbed?reportId={reportId}&groupId={workspaceId}",
            EmbedToken = $"MOCK.{Convert.ToBase64String(Guid.NewGuid().ToByteArray())}.{effectiveIdentity.Username}",
            TokenExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
            EmbedType = "report",
        };

        return Task.FromResult(config);
    }
}
