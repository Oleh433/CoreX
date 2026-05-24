using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Memberships;

public class BookTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public BookTests(CoreXFactory factory) => _factory = factory;

    private async Task<Guid> SubscriptionIdAsync(Guid clubId)
    {
        using var scope = _factory.Services.CreateScope();
        var subs = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var list = await subs.GetByClubIdAsync(clubId);
        return list[0].Id;
    }

    [Fact]
    public async Task Get_Book_AnonymousShowsEmptyForm()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Memberships/{subId}/Book");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.ContactFullName\"", body);
        Assert.Contains("name=\"Input.ContactEmail\"", body);
        Assert.Contains("name=\"Input.ContactPhone\"", body);
        Assert.Contains("__RequestVerificationToken", body);
        Assert.Contains("Місячний", body); // subscription title surfaced on form
    }

    [Fact]
    public async Task Post_Book_Anonymous_CreatesBooking_AndRedirectsToConfirmed()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
        var post = AntiforgeryClient.BuildPost(
            $"/Memberships/{subId}/Book",
            new Dictionary<string, string>
            {
                ["Input.ContactFullName"] = "Анонім Тест",
                ["Input.ContactEmail"] = "anon@test",
                ["Input.ContactPhone"] = "+380501234567",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Memberships/Confirmed", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Book_Authenticated_CreatesBookingWithUserId()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);

        var email = $"booker-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Тарас Шевченко");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
        var post = AntiforgeryClient.BuildPost(
            $"/Memberships/{subId}/Book",
            new Dictionary<string, string>
            {
                ["Input.ContactFullName"] = "Тарас Шевченко",
                ["Input.ContactEmail"] = email,
                ["Input.ContactPhone"] = "+380501234567",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Memberships/Confirmed", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Book_WithMissingPhone_ReturnsForm_WithError()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var subId = await SubscriptionIdAsync(clubs[0].Id);

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Memberships/{subId}/Book");
        var post = AntiforgeryClient.BuildPost(
            $"/Memberships/{subId}/Book",
            new Dictionary<string, string>
            {
                ["Input.ContactFullName"] = "Анонім",
                ["Input.ContactEmail"] = "a@b",
                ["Input.ContactPhone"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Введіть телефон", body);
    }
}
