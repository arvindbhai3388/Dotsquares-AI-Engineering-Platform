using ClientReportingPortal.Web.Contracts.SharePoint;
using ClientReportingPortal.Web.Controllers;
using ClientReportingPortal.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ClientReportingPortal.Tests.Controllers;

public class DocumentsControllerTests
{
    private readonly Mock<ISharePointDocumentService> _documentServiceMock = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder().Build();

    [Fact]
    public async Task Index_ReturnsViewWithDocumentsFromService()
    {
        var documents = new List<SharePointDocument>
        {
            new()
            {
                Id = "1",
                Name = "a.pdf",
                SizeInBytes = 10,
                LastModifiedUtc = DateTimeOffset.UtcNow,
                LastModifiedBy = "Someone",
                WebUrl = "https://example.test/a.pdf",
                ContentType = "application/pdf",
            },
        };
        _documentServiceMock
            .Setup(s => s.ListDocumentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        var sut = new DocumentsController(_documentServiceMock.Object, _configuration)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()),
        };

        var result = await sut.Index(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DocumentsViewModel>(viewResult.Model);
        Assert.Same(documents, model.Documents);
    }

    [Fact]
    public async Task Upload_RedirectsToIndex_WhenNoFileProvided()
    {
        var sut = new DocumentsController(_documentServiceMock.Object, _configuration)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()),
        };

        var result = await sut.Upload(file: null, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        _documentServiceMock.Verify(
            s => s.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Upload_CallsServiceAndRedirects_WhenFileProvided()
    {
        var uploaded = new SharePointDocument
        {
            Id = "new-1",
            Name = "upload.txt",
            SizeInBytes = 5,
            LastModifiedUtc = DateTimeOffset.UtcNow,
            LastModifiedBy = "Demo User",
            WebUrl = "https://example.test/upload.txt",
            ContentType = "text/plain",
        };
        _documentServiceMock
            .Setup(s => s.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), "upload.txt", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploaded);

        var sut = new DocumentsController(_documentServiceMock.Object, _configuration)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()),
        };

        var content = "hello"u8.ToArray();
        var formFile = new FormFile(new MemoryStream(content), 0, content.Length, "file", "upload.txt");

        var result = await sut.Upload(formFile, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        _documentServiceMock.Verify(
            s => s.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), "upload.txt", It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Upload_RejectsOversizedFile_WithoutCallingService()
    {
        var sut = new DocumentsController(_documentServiceMock.Object, _configuration)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()),
        };

        const int oversizedLength = (10 * 1024 * 1024) + 1; // one byte over the 10 MB limit
        var formFile = new FormFile(Stream.Null, 0, oversizedLength, "file", "too-big.pdf");

        var result = await sut.Upload(formFile, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        Assert.IsAssignableFrom<ProblemDetails>(problemResult.Value);
        _documentServiceMock.Verify(
            s => s.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Upload_RejectsDisallowedExtension_WithoutCallingService()
    {
        var sut = new DocumentsController(_documentServiceMock.Object, _configuration)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()),
        };

        var content = "malicious"u8.ToArray();
        var formFile = new FormFile(new MemoryStream(content), 0, content.Length, "file", "payload.exe");

        var result = await sut.Upload(formFile, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        Assert.IsAssignableFrom<ProblemDetails>(problemResult.Value);
        _documentServiceMock.Verify(
            s => s.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Upload_AcceptsValidFile_WithinSizeAndExtensionAllowlist()
    {
        var uploaded = new SharePointDocument
        {
            Id = "new-2",
            Name = "report.pdf",
            SizeInBytes = 5,
            LastModifiedUtc = DateTimeOffset.UtcNow,
            LastModifiedBy = "Demo User",
            WebUrl = "https://example.test/report.pdf",
            ContentType = "application/pdf",
        };
        _documentServiceMock
            .Setup(s => s.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), "report.pdf", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploaded);

        var sut = new DocumentsController(_documentServiceMock.Object, _configuration)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()),
        };

        var content = "hello"u8.ToArray();
        var formFile = new FormFile(new MemoryStream(content), 0, content.Length, "file", "report.pdf");

        var result = await sut.Upload(formFile, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        _documentServiceMock.Verify(
            s => s.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), "report.pdf", It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
