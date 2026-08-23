namespace SharedComponents.Models;

/// <summary>
/// Theme/settings value flowed down the component tree via a <c>CascadingValue</c>
/// (see <c>DashboardHost/Components/Layout/MainLayout.razor</c>), demonstrating the
/// cascading-parameters pattern referenced in the framework's Blazor wiki page.
/// </summary>
/// <param name="Name">Display name of the current theme, e.g. "Dark Ops".</param>
/// <param name="AccentColor">CSS color used for accents/highlights.</param>
/// <param name="IsDark">Whether the theme is a dark theme.</param>
public record ThemeSettings(string Name, string AccentColor, bool IsDark);
