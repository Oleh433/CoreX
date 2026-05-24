using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Account;

public class RegisterTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public RegisterTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Register_ReturnsOk_AndRendersForm()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Account/Register");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.FullName\"", body);
        Assert.Contains("name=\"Input.Email\"", body);
        Assert.Contains("name=\"Input.Password\"", body);
        Assert.Contains("name=\"Input.ConfirmPassword\"", body);
        Assert.Contains("name=\"Input.TermsAccepted\"", body);
    }

    [Fact]
    public async Task Post_Register_WithValidInput_CreatesUserAndAutoLogsIn()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var email = $"reg-{Guid.NewGuid():N}@test";
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Test Person",
                ["Input.Email"] = email,
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "ValidPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Post_Register_WithMismatchedPasswords_ReturnsForm_WithError()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Test Person",
                ["Input.Email"] = $"reg-mm-{Guid.NewGuid():N}@test",
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "DifferentPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Паролі не співпадають.", body);
    }

    [Fact]
    public async Task Post_Register_WithTermsNotAccepted_ReturnsForm_WithError()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Test Person",
                ["Input.Email"] = $"reg-terms-{Guid.NewGuid():N}@test",
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "ValidPass1!",
                ["Input.TermsAccepted"] = "false",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Потрібно прийняти умови використання.", body);
    }

    [Fact]
    public async Task Post_Register_WithDuplicateEmail_ReturnsForm_WithError()
    {
        var email = $"reg-dup-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Register");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Register",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Other Person",
                ["Input.Email"] = email,
                ["Input.Password"] = "ValidPass1!",
                ["Input.ConfirmPassword"] = "ValidPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Користувач з такою електронною адресою вже існує.", body);
    }
}
