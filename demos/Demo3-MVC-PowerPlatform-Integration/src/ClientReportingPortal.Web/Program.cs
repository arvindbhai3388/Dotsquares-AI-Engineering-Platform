using ClientReportingPortal.Web.Contracts.PowerBi;
using ClientReportingPortal.Web.Contracts.SharePoint;
using ClientReportingPortal.Web.Contracts.Tasks;
using ClientReportingPortal.Web.Services.PowerBi;
using ClientReportingPortal.Web.Services.SharePoint;
using ClientReportingPortal.Web.Services.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- Power Platform / Microsoft 365 integration seams ---------------------------------------
// Every integration is registered against its interface only. Swapping "Mock..." for a real
// implementation (see each interface's XML doc for exactly what that involves) is a one-line
// change here - no controller or view changes required. See this demo's README for the full
// "mock now / real later" explanation per integration.
builder.Services.AddSingleton<IPowerBiEmbedService, MockPowerBiEmbedService>();
builder.Services.AddSingleton<ISharePointDocumentService, MockSharePointDocumentService>();
builder.Services.AddSingleton<ITaskService, InMemoryTaskService>();

// --- Power Apps custom-connector-shaped API surface (/api/tasks) ----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Client Reporting Portal - Tasks API",
        Version = "v1",
        Description = "CRUD surface shaped for import into Power Apps as a custom connector. " +
                      "See the demo README for the OpenAPI import / Dataverse notes.",
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasks API v1");
    });
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Exposed for WebApplicationFactory<Program> in the companion test project.
public partial class Program { }
