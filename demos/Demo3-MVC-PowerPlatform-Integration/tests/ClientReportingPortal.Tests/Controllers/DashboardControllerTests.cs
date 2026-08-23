using ClientReportingPortal.Web.Contracts.PowerBi;
using ClientReportingPortal.Web.Controllers;
using ClientReportingPortal.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ClientReportingPortal.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IPowerBiEmbedService> _powerBiEmbedServiceMock = new();

    private static IConfiguration BuildConfiguration(IDictionary<string, string?>? values = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(values ?? new Dictionary<string, string?>()).Build();

    [Fact]
    public async Task Index_ReturnsViewWithEmbedConfigFromService()
    {
        var expectedConfig = new EmbedConfig
        {
            ReportId = "report-1",
            WorkspaceId = "workspace-1",
            EmbedUrl = "https://app.powerbi.com/reportEmbed?reportId=report-1&groupId=workspace-1",
            EmbedToken = "token-value",
            TokenExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
        };
        _powerBiEmbedServiceMock
            .Setup(s => s.GetEmbedTokenAsync("report-1", "workspace-1", It.IsAny<EffectiveIdentity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedConfig);

        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["PowerBi:ReportId"] = "report-1",
            ["PowerBi:WorkspaceId"] = "workspace-1",
        });

        var sut = new DashboardController(_powerBiEmbedServiceMock.Object, configuration);

        var result = await sut.Index(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DashboardViewModel>(viewResult.Model);
        Assert.Same(expectedConfig, model.EmbedConfig);
    }

    [Fact]
    public async Task Index_FallsBackToPlaceholderIds_WhenConfigurationMissing()
    {
        _powerBiEmbedServiceMock
            .Setup(s => s.GetEmbedTokenAsync(
                "00000000-0000-0000-0000-000000000000",
                "00000000-0000-0000-0000-000000000000",
                It.IsAny<EffectiveIdentity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmbedConfig
            {
                ReportId = "00000000-0000-0000-0000-000000000000",
                WorkspaceId = "00000000-0000-0000-0000-000000000000",
                EmbedUrl = "https://app.powerbi.com/reportEmbed",
                EmbedToken = "token",
                TokenExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
            });

        var sut = new DashboardController(_powerBiEmbedServiceMock.Object, BuildConfiguration());

        var result = await sut.Index(CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        _powerBiEmbedServiceMock.VerifyAll();
    }
}
