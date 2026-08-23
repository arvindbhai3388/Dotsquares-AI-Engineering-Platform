using ClientReportingPortal.Web.Contracts.SharePoint;
using ClientReportingPortal.Web.Contracts.Tasks;
using ClientReportingPortal.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientReportingPortal.Web.Controllers;

/// <summary>
/// Read-only admin page summarizing which integration seam each interface is currently wired
/// to (always "Mock" in this demo) and pointing at where the real implementation notes live.
/// </summary>
public sealed class AdminController : Controller
{
    private readonly ISharePointDocumentService _documentService;
    private readonly ITaskService _taskService;

    public AdminController(ISharePointDocumentService documentService, ITaskService taskService)
    {
        _documentService = documentService;
        _taskService = taskService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetAllAsync(cancellationToken);
        var documents = await _documentService.ListDocumentsAsync("demo-site", "demo-drive", cancellationToken);

        var viewModel = new AdminViewModel
        {
            TaskCount = tasks.Count,
            DocumentCount = documents.Count,
            Integrations = new[]
            {
                new IntegrationStatusRow(
                    "Power BI Embedded Analytics",
                    "IPowerBiEmbedService",
                    "MockPowerBiEmbedService",
                    "RealPowerBiEmbedService would use Azure.Identity + Microsoft.PowerBI.Api against the Power BI REST GenerateToken endpoint."),
                new IntegrationStatusRow(
                    "SharePoint Documents",
                    "ISharePointDocumentService",
                    "MockSharePointDocumentService",
                    "RealSharePointDocumentService would use the Microsoft.Graph SDK with app-only auth (Microsoft.Identity.Client) and Sites.Read.All/Sites.ReadWrite.All scopes."),
                new IntegrationStatusRow(
                    "Power Apps Custom Connector Target",
                    "ITaskService (backing /api/tasks)",
                    "InMemoryTaskService",
                    "The /api/tasks OpenAPI document (see /swagger) can be imported directly into Power Apps as a custom connector; swap this for an EF Core/SQL or Dataverse-backed implementation for production."),
            },
        };

        return View(viewModel);
    }
}
