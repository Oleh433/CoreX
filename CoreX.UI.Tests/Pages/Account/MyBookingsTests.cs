using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Account;

public class MyBookingsTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public MyBookingsTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_MyBookings_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/Account/MyBookings");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        // Folder-auth challenge sends an absolute URL (e.g. "http://localhost/Account/Login?ReturnUrl=...");
        // assert via AbsolutePath so the scheme/host prefix doesn't break the prefix check.
        Assert.StartsWith("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Get_MyBookings_AuthenticatedWithNoBookings_RendersEmptyState()
    {
        var email = $"mb-empty-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Account/MyBookings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Поки що бронювань немає.", body);
    }
}
