using System.Net;
using CoreX.Domain.IdentityEntities;
using CoreX.UI.Tests.TestSupport;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Users;

public class RegisterAdminTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public RegisterAdminTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_RegisterAdmin_AsAdmin_RedirectsAway()
    {
        // Admin (not Owner) must not reach the Owner-only RegisterAdmin page.
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Users/RegisterAdmin");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_RegisterAdmin_AsOwner_RendersForm()
    {
        var client = await TestUsers.SignedInAsOwnerAsync(_factory);

        var response = await client.GetAsync("/Admin/Users/RegisterAdmin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Додати адміна", body);
        Assert.Contains("Input.Email", body);
    }

    [Fact]
    public async Task Post_RegisterAdmin_AsOwner_CreatesAdmin()
    {
        var newAdminEmail = $"new-admin-{Guid.NewGuid():N}@test";

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Users/RegisterAdmin");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Users/RegisterAdmin",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Новий Адмін",
                ["Input.Email"] = newAdminEmail,
                ["Input.Password"] = "AdminPass1!",
                ["Input.ConfirmPassword"] = "AdminPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var created = await userManager.FindByEmailAsync(newAdminEmail);
        Assert.NotNull(created);
        Assert.True(await userManager.IsInRoleAsync(created!, "Admin"));
    }

    [Fact]
    public async Task Post_RegisterAdmin_AsOwner_DuplicateEmail_ReturnsForm_WithError()
    {
        var existingEmail = $"existing-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, existingEmail, role: "User");

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Users/RegisterAdmin");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Users/RegisterAdmin",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Дублікат Адмін",
                ["Input.Email"] = existingEmail,
                ["Input.Password"] = "AdminPass1!",
                ["Input.ConfirmPassword"] = "AdminPass1!",
                ["Input.TermsAccepted"] = "true",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        // Mapped UA error: "Користувач з такою електронною адресою вже існує."
        // Use a prefix substring to dodge any apostrophe / encoding quirks.
        Assert.Contains("Користувач з такою електронною адресою", body);
    }
}
