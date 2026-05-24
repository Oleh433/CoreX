using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Account;

public class LoginTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public LoginTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Login_ReturnsOk_AndRendersForm()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.Email\"", body);
        Assert.Contains("name=\"Input.Password\"", body);
        Assert.Contains("__RequestVerificationToken", body);
        Assert.Contains("Увійти", body);
    }

    [Fact]
    public async Task Post_Login_WithValidCredentials_RedirectsAndSetsAuthCookie()
    {
        var email = $"login-ok-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = TestUsers.DefaultPassword,
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Post_Login_WithInvalidPassword_ReturnsForm_WithError()
    {
        var email = $"login-bad-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "WRONG-password-1",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Невірна електронна адреса або пароль.", body);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var setCookies)
            && setCookies.Any(c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal)),
            "No auth cookie should be set on failed sign-in.");
    }

    [Fact]
    public async Task Post_Login_WithReturnUrl_RedirectsToReturnUrl()
    {
        var email = $"login-ret-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login?ReturnUrl=%2FAccount%2FProfile");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login?ReturnUrl=%2FAccount%2FProfile",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = TestUsers.DefaultPassword,
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Account/Profile", response.Headers.Location?.OriginalString);
    }
}
