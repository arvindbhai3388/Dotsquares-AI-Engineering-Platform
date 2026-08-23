using ClientReportingPortal.Web.Contracts.SharePoint;
using ClientReportingPortal.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientReportingPortal.Web.Controllers;

/// <summary>
/// Lists and uploads documents from a SharePoint document library. Talks only to
/// <see cref="ISharePointDocumentService"/> - see that interface's XML doc and
/// <c>Services/SharePoint/MockSharePointDocumentService.cs</c> for the mock-now/real-later
/// seam this page demonstrates.
/// </summary>
public sealed class DocumentsController : Controller
{
    // Guardrails only - see the comment on Upload() for why a real (non-mock) implementation
    // must enforce the same checks server-side before forwarding to Microsoft Graph.
    private const long MaxUploadSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedUploadExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv",
    };

    private readonly ISharePointDocumentService _documentService;
    private readonly IConfiguration _configuration;

    public DocumentsController(ISharePointDocumentService documentService, IConfiguration configuration)
    {
        _documentService = documentService;
        _configuration = configuration;
    }

    private (string SiteId, string DriveId) GetSiteAndDrive() =>
        (_configuration["SharePoint:SiteId"] ?? "<TENANT_SHAREPOINT_SITE_ID>",
         _configuration["SharePoint:DriveId"] ?? "<TENANT_SHAREPOINT_DRIVE_ID>");

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var (siteId, driveId) = GetSiteAndDrive();
        var documents = await _documentService.ListDocumentsAsync(siteId, driveId, cancellationToken);

        return View(new DocumentsViewModel
        {
            Documents = documents,
            StatusMessage = TempData["StatusMessage"] as string,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        var (siteId, driveId) = GetSiteAndDrive();

        if (file is null || file.Length == 0)
        {
            TempData["StatusMessage"] = "Choose a file before uploading.";
            return RedirectToAction(nameof(Index));
        }

        // Basic size/type guardrail before touching the (mock) service. A real, non-mock
        // ISharePointDocumentService must apply the same checks again server-side before
        // forwarding the stream to Microsoft Graph - client-declared size/content-type/extension
        // are not trustworthy on their own (Graph itself has its own upload-size limits per method,
        // e.g. the ~4 MB simple PUT vs. an upload session for larger files).
        if (file.Length > MaxUploadSizeBytes)
        {
            return Problem(
                detail: $"'{file.FileName}' is {file.Length:N0} bytes, which exceeds the {MaxUploadSizeBytes / (1024 * 1024)} MB upload limit.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "File too large");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedUploadExtensions.Contains(extension))
        {
            return Problem(
                detail: $"'{file.FileName}' has an unsupported file type. Allowed extensions: {string.Join(", ", AllowedUploadExtensions.Order())}.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unsupported file type");
        }

        await using var stream = file.OpenReadStream();
        var uploaded = await _documentService.UploadDocumentAsync(siteId, driveId, file.FileName, stream, cancellationToken);

        TempData["StatusMessage"] = $"Uploaded '{uploaded.Name}' ({uploaded.SizeInBytes:N0} bytes).";
        return RedirectToAction(nameof(Index));
    }
}
