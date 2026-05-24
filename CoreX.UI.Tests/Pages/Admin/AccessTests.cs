using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin;

public class AccessTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public AccessTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminIndex_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Get_AdminIndex_UserRole_IsForbiddenOrRedirected()
    {
        var email = $"user-noadmin-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Admin");

        // Identity default: 302 to /Account/AccessDenied (or 403 if configured).
        // Either way, NOT 200.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_AdminIndex_AdminRole_ReturnsOk_WithSidebar()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var response = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Адмін-панель", body);
        Assert.Contains("Клуби", body);   // sidebar link
        Assert.Contains("Бронювання", body); // sidebar link
    }

    [Fact]
    public async Task Get_AdminIndex_OwnerRole_ReturnsOk()
    {
        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var response = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
