using System.Net;
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Subscriptions;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminSubscriptions_AsAdmin_RedirectsAway()
    {
        // Admin (not Owner) must not reach the Owner-only /Admin/Subscriptions page.
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Subscriptions");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_AdminSubscriptions_AsOwner_ListsSubscriptions()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = await TestUsers.SignedInAsOwnerAsync(_factory);

        var response = await client.GetAsync("/Admin/Subscriptions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Місячний", body);
        Assert.Contains("Квартальний", body);
    }

    [Fact]
    public async Task Get_AdminSubscriptions_Create_AsAdmin_RedirectsAway()
    {
        // Admin (not Owner) must not reach the Owner-only Create page either.
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Subscriptions/Create");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_AdminSubscriptions_Create_AsOwner_CreatesSubscription()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubAId = seeded.First(c => c.Name == "Energy Kyiv").Id;

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Subscriptions/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Subscriptions/Create",
            new Dictionary<string, string>
            {
                ["Input.ClubId"] = clubAId.ToString(),
                ["Input.Title"] = "Річний абонемент",
                ["Input.Price"] = "9000",
                ["Input.DurationDays"] = "365",
                ["Input.Description"] = "Безліміт на цілий рік.",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/Subscriptions", response.Headers.Location?.AbsolutePath);

        using var scope = _factory.Services.CreateScope();
        var subs = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var all = await subs.GetAllAsync();
        Assert.Contains(all, s => s.Title == "Річний абонемент");
    }

    [Fact]
    public async Task Post_AdminSubscriptions_Edit_AsOwner_UpdatesSubscription()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Throwaway subscription — don't mutate seeded fixture other tests use.
        Guid subId;
        using (var scope = _factory.Services.CreateScope())
        {
            var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
            var clubAId = (await clubs.GetAllAsync()).First(c => c.Name == "Energy Kyiv").Id;

            var subs = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            subId = await subs.CreateAsync(new CreateSubscriptionDto
            {
                ClubId = clubAId,
                Title = "До редагування",
                Price = 500m,
                DurationDays = 30,
                Description = "Початковий опис.",
            });
        }

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Admin/Subscriptions/{subId}/Edit");
        var post = AntiforgeryClient.BuildPost(
            $"/Admin/Subscriptions/{subId}/Edit",
            new Dictionary<string, string>
            {
                ["Input.Title"] = "Оновлений абонемент",
                ["Input.Price"] = "750",
                ["Input.DurationDays"] = "45",
                ["Input.Description"] = "Оновлений опис.",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        using var scope2 = _factory.Services.CreateScope();
        var subsAfter = scope2.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var updated = await subsAfter.GetByIdAsync(subId);
        Assert.NotNull(updated);
        Assert.Equal("Оновлений абонемент", updated!.Title);
    }

    [Fact]
    public async Task PostHx_AdminSubscriptions_Deactivate_AsOwner_FlipsIsActive()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Throwaway active subscription created in the test.
        Guid subId;
        using (var scope = _factory.Services.CreateScope())
        {
            var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
            var clubAId = (await clubs.GetAllAsync()).First(c => c.Name == "Energy Kyiv").Id;

            var subs = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            subId = await subs.CreateAsync(new CreateSubscriptionDto
            {
                ClubId = clubAId,
                Title = "Активний до деактивації",
                Price = 600m,
                DurationDays = 30,
            });
        }

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Subscriptions");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Subscriptions?handler=Deactivate&id={subId}");
        req.Headers.Add("HX-Request", "true");
        if (!string.IsNullOrEmpty(afCookie))
        {
            req.Headers.Add("Cookie", afCookie);
        }
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
        });

        var response = await client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Неактивний", body);

        using var scopeAfter = _factory.Services.CreateScope();
        var subsAfter = scopeAfter.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var refetched = await subsAfter.GetByIdAsync(subId);
        Assert.NotNull(refetched);
        Assert.False(refetched!.IsActive);
    }

    [Fact]
    public async Task PostHx_AdminSubscriptions_Delete_AsOwner_RemovesSubscription()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Throwaway subscription.
        Guid subId;
        using (var scope = _factory.Services.CreateScope())
        {
            var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
            var clubAId = (await clubs.GetAllAsync()).First(c => c.Name == "Energy Kyiv").Id;

            var subs = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            subId = await subs.CreateAsync(new CreateSubscriptionDto
            {
                ClubId = clubAId,
                Title = "До видалення",
                Price = 400m,
                DurationDays = 30,
            });
        }

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Subscriptions");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Subscriptions?handler=Delete&id={subId}");
        req.Headers.Add("HX-Request", "true");
        if (!string.IsNullOrEmpty(afCookie))
        {
            req.Headers.Add("Cookie", afCookie);
        }
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
        });

        var response = await client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scopeAfter = _factory.Services.CreateScope();
        var subsAfter = scopeAfter.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var stillThere = await subsAfter.GetByIdAsync(subId);
        Assert.Null(stillThere);
    }
}
