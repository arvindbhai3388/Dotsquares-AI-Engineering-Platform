namespace ClientReportingPortal.Web.Models;

/// <summary>One row of the admin "integration status" table.</summary>
public sealed record IntegrationStatusRow(
    string IntegrationName,
    string Interface,
    string ActiveImplementation,
    string RealImplementationNote);

/// <summary>View model for the admin page's integration-status overview.</summary>
public sealed class AdminViewModel
{
    public required IReadOnlyList<IntegrationStatusRow> Integrations { get; init; }

    public required int TaskCount { get; init; }

    public required int DocumentCount { get; init; }
}
