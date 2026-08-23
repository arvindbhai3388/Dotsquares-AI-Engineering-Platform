using ClientReportingPortal.Web.Contracts.PowerBi;
using ClientReportingPortal.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientReportingPortal.Web.Controllers;

/// <summary>
/// Renders the embedded Power BI report. Talks only to <see cref="IPowerBiEmbedService"/> -
/// see that interface's XML doc and <c>Services/PowerBi/MockPowerBiEmbedService.cs</c> for the
/// mock-now/real-later seam this page demonstrates.
/// </summary>
public sealed class DashboardController : Controller
{
    private readonly IPowerBiEmbedService _powerBiEmbedService;
    private readonly IConfiguration _configuration;

    public DashboardController(IPowerBiEmbedService powerBiEmbedService, IConfiguration configuration)
    {
        _powerBiEmbedService = powerBiEmbedService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var reportId = _configuration["PowerBi:ReportId"] ?? "00000000-0000-0000-0000-000000000000";
        var workspaceId = _configuration["PowerBi:WorkspaceId"] ?? "00000000-0000-0000-0000-000000000000";
        var username = User?.Identity?.Name ?? "demo.user@contoso.example";

        // In a real deployment this role list would come from the signed-in user's claims,
        // not be hardcoded - see IPowerBiEmbedService's XML doc for the RLS contract.
        var effectiveIdentity = new EffectiveIdentity(username, Roles: new[] { "ClientReader" }, DatasetIds: new[] { workspaceId });

        var embedConfig = await _powerBiEmbedService.GetEmbedTokenAsync(reportId, workspaceId, effectiveIdentity, cancellationToken);

        var viewModel = new DashboardViewModel
        {
            EmbedConfig = embedConfig,
            ReportTitle = "Client Engagement Overview",
            EffectiveIdentityUsername = username,
        };

        return View(viewModel);
    }
}
