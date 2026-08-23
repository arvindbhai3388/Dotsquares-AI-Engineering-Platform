using DashboardHost.Components;
using DashboardHost.Hubs;
using DashboardHost.Services;
using SharedComponents.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

// Metrics simulation: a singleton generator/settings store shared by every hub connection,
// plus the background service that ticks and broadcasts on the configured interval.
builder.Services.AddSingleton<IMetricsGenerator, MetricsGenerator>();
builder.Services.AddSingleton<DashboardSettingsService>();
builder.Services.AddHostedService<MetricsBroadcastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<MetricsHub>("/hubs/metrics");

app.Run();

/// <summary>
/// Exposes the generated <c>Program</c> class so <c>WebApplicationFactory&lt;Program&gt;</c>
/// can be used from the companion DashboardHost.Tests project.
/// </summary>
public partial class Program
{
}
