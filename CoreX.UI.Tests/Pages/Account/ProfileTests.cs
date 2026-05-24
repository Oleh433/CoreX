using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Account;

public class ProfileTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public ProfileTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Profile_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/Account/Profile");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        // Folder-auth challenge sends an absolute URL (e.g. "http://localhost/Account/Login?ReturnUrl=...");
        // assert via AbsolutePath so the scheme/host prefix doesn't break the prefix check.
        Assert.StartsWith("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Get_Profile_Authenticated_ShowsFullNameAndEmail()
    {
        var email = $"profile-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Olha Bilash");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Account/Profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Olha Bilash", body);
        Assert.Contains(email, body);
    }
}
