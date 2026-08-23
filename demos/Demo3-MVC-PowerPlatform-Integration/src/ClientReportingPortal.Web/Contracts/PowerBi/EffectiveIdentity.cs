namespace ClientReportingPortal.Web.Contracts.PowerBi;

/// <summary>
/// Row-level-security (RLS) identity that a real Power BI "GenerateToken" call would forward
/// so the embedded report only shows data the signed-in portal user is allowed to see.
/// See: https://learn.microsoft.com/power-bi/enterprise/service-admin-rls
/// </summary>
/// <param name="Username">
/// The identity string Power BI matches against RLS role rules - typically the caller's UPN or a tenant/client key,
/// never a display name.
/// </param>
/// <param name="Roles">The RLS role name(s), defined on the dataset, that should be applied for this user.</param>
/// <param name="DatasetIds">Datasets the roles apply to (a report can be built on more than one dataset).</param>
public sealed record EffectiveIdentity(string Username, IReadOnlyList<string> Roles, IReadOnlyList<string> DatasetIds);
