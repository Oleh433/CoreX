using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages;

public class ErrorTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public ErrorTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_UnknownUrl_RendersLocalizedNotFoundPage()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/this-url-does-not-exist-xyz");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Сторінку не знайдено", body);
    }

    [Fact]
    public async Task Get_AdminPath_AsUser_RendersAccessDeniedPage()
    {
        var email = $"user-403-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        // SignedInClientAsync returns a client with AllowAutoRedirect=false, so we
        // walk the redirect chain manually: /Admin -> /Error/403 (after Phase 6,
        // IdentityOptions.AccessDeniedPath = "/Error/403").
        var deniedRedirect = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Redirect, deniedRedirect.StatusCode);
        var location = deniedRedirect.Headers.Location!.ToString();
        Assert.Contains("/Error/403", location);

        var deniedPage = await client.GetAsync(location);
        var body = await deniedPage.Content.ReadAsStringAsync();
        Assert.Contains("Доступ заборонено", body);
    }
}
