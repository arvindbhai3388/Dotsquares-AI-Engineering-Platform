using ClientReportingPortal.Web.Contracts.PowerBi;

namespace ClientReportingPortal.Web.Models;

/// <summary>View model for the Power BI embedded-analytics dashboard page.</summary>
public sealed class DashboardViewModel
{
    public required EmbedConfig EmbedConfig { get; init; }

    public required string ReportTitle { get; init; }

    public required string EffectiveIdentityUsername { get; init; }
}
