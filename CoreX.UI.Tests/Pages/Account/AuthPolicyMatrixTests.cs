using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Account;

public class AuthPolicyMatrixTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public AuthPolicyMatrixTests(CoreXFactory factory) => _factory = factory;

    public static IEnumerable<object[]> AnonymousCases() =>
        new[]
        {
            new object[] { "/Account/Login",      HttpStatusCode.OK },
            new object[] { "/Account/Register",   HttpStatusCode.OK },
            new object[] { "/Account/Profile",    HttpStatusCode.Found },
            new object[] { "/Account/MyBookings", HttpStatusCode.Found },
            new object[] { "/Account/Logout",     HttpStatusCode.Found },
        };

    [Theory]
    [MemberData(nameof(AnonymousCases))]
    public async Task Anonymous_AccessToAccountPages_BehavesAsExpected(string url, HttpStatusCode expected)
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync(url);

        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.Found)
            // Folder-auth challenge emits an absolute URL (e.g. "http://localhost/Account/Login?ReturnUrl=...");
            // assert via AbsolutePath so the scheme/host prefix doesn't break the prefix check.
            Assert.StartsWith("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Owner")]
    public async Task AuthenticatedUser_CanLoadOwnAccountPages(string role)
    {
        var email = $"matrix-{role.ToLowerInvariant()}-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: role);
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        foreach (var page in new[] { "/Account/Profile", "/Account/MyBookings" })
        {
            var response = await client.GetAsync(page);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
