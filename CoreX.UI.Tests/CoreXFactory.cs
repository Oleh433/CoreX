using System.Linq;
using CoreX.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.UI.Tests;

public class CoreXFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Owner:Email"] = "owner@corex.test",
                ["Owner:Password"] = "TestOwnerPass1!",
                ["ConnectionStrings:DatabaseConnectionString"] = "ignored-by-tests"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(o =>
                o.UseInMemoryDatabase($"CoreXTests-{Guid.NewGuid()}"));
        });
    }
}
