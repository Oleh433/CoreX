using System.Net;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task Get_MyBookings_AuthenticatedWithBookings_ShowsBookingRow()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Resolve a subscription
        Guid subId;
        using (var scope = _factory.Services.CreateScope())
        {
            var subs = scope.ServiceProvider.GetRequiredService<CoreX.Application.ServiceInterfaces.ISubscriptionService>();
            var list = await subs.GetByClubIdAsync(clubs[0].Id);
            subId = list[0].Id;
        }

        var email = $"mb-pop-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Бронер Тест");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        // POST a booking through the public form
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
        var post = AntiforgeryClient.BuildPost(
            $"/Memberships/{subId}/Book",
            new Dictionary<string, string>
            {
                ["Input.ContactFullName"] = "Бронер Тест",
                ["Input.ContactEmail"] = email,
                ["Input.ContactPhone"] = "+380501234567",
            },
            token, afCookie);
        var bookResponse = await client.SendAsync(post);
        Assert.Equal(System.Net.HttpStatusCode.Found, bookResponse.StatusCode);

        // Now /Account/MyBookings should show the booking
        var response = await client.GetAsync("/Account/MyBookings");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Місячний", body); // subscription Title
        Assert.Contains("Energy Kyiv", body); // club Name
        Assert.DoesNotContain("Поки що бронювань немає.", body);
    }
}
