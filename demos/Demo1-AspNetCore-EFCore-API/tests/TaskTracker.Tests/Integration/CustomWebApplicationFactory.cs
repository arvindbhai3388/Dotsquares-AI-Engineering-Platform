using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Api.Data;

namespace TaskTracker.Tests.Integration;

/// <summary>
/// Swaps the real SQL Server <see cref="TaskTrackerDbContext"/> registration for an
/// EF Core InMemory provider, so integration tests run without LocalDB or any
/// external SQL Server instance.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TaskTrackerDbContext>));
            if (dbContextOptionsDescriptor is not null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            services.AddDbContext<TaskTrackerDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
