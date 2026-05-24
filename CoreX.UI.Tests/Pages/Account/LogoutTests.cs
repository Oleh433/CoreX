using System.Net;
using System.Text.RegularExpressions;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Account;

public class LogoutTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public LogoutTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Logout_Anonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync("/Account/Logout");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        // Folder-auth challenge sends an absolute URL (e.g. "http://localhost/Account/Login?ReturnUrl=...");
        // assert via AbsolutePath so the scheme/host prefix doesn't break the prefix check.
        Assert.StartsWith("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Logout_Authenticated_RedirectsHome_AndClearsAuthCookie()
    {
        var email = $"logout-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        // The signed-in client already has the antiforgery cookie in its handler's cookie
        // jar from the sign-in flow; we just need to scrape a fresh token from any page
        // (the layout emits one on every request). The handler will auto-attach the cookie.
        var page = await client.GetAsync("/");
        var html = await page.Content.ReadAsStringAsync();
        var tokenMatch = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""(?<t>[^""]+)""");
        Assert.True(tokenMatch.Success, "Expected an antiforgery token in the layout-emitted form.");

        var post = new HttpRequestMessage(HttpMethod.Post, "/Account/Logout")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", tokenMatch.Groups["t"].Value),
            }),
        };
        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal));
    }
}
