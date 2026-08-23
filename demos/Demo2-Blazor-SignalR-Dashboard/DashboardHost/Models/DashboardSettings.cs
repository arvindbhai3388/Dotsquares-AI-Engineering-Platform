using System.ComponentModel.DataAnnotations;

namespace DashboardHost.Models;

/// <summary>
/// Editable dashboard settings, bound by <c>Components/Pages/Settings.razor</c> via
/// <c>EditForm</c> + <see cref="DataAnnotationsValidator"/>. Kept separate from
/// <see cref="SharedComponents.Models.ThemeSettings"/>, which is display-only and cascaded,
/// not user-editable.
/// </summary>
public class DashboardSettings
{
    /// <summary>How often, in seconds, the background service broadcasts a new metric reading.</summary>
    [Range(2, 60, ErrorMessage = "Refresh interval must be between 2 and 60 seconds.")]
    public int RefreshIntervalSeconds { get; set; } = 5;
}
