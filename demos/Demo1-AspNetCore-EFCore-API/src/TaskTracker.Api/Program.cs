using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Hubs;
using TaskTracker.Api.Options;
using TaskTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Serialize enums (e.g. TaskItemStatus) as their string names rather than integers,
// so the REST API is self-describing over the wire (request and response bodies alike).
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Options pattern for configurable pagination bounds (bound from the "Pagination" config section).
builder.Services.AddOptions<PaginationOptions>()
    .Bind(builder.Configuration.GetSection(PaginationOptions.SectionName))
    .ValidateDataAnnotations();

// EF Core Code-First against SQL Server / LocalDB. Connection string comes from configuration
// (appsettings.json / appsettings.Development.json / environment) — never hardcoded.
var connectionString = builder.Configuration.GetConnectionString("TaskTrackerDb")
    ?? throw new InvalidOperationException("Connection string 'TaskTrackerDb' is not configured.");

builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseSqlServer(connectionString));

// Business-logic services.
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskItemService, TaskItemService>();

// SignalR for real-time task notifications.
builder.Services.AddSignalR();
builder.Services.AddScoped<ITaskHubNotifier, TaskHubNotifier>();

// Uniform ProblemDetails responses for validation and unhandled-exception scenarios.
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await problemDetailsService.WriteAsync(new ProblemDetailsContext
    {
        HttpContext = context,
        ProblemDetails =
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
        }
    });
}));

app.UseHttpsRedirection();

// Serves wwwroot/signalr-test.html — a small manual test client for TaskHub (see README).
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<TaskHub>("/hubs/tasks");

app.Run();

/// <summary>
/// Exposes the generated <c>Program</c> class so <c>WebApplicationFactory&lt;Program&gt;</c>
/// can be used from the companion integration test project.
/// </summary>
public partial class Program
{
}
