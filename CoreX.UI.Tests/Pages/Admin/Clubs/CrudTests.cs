using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Clubs;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminClubs_AsAdmin_ListsClubs()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Clubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Energy Kyiv", body);
        Assert.Contains("Forge Lviv", body);
    }

    [Fact]
    public async Task Post_AdminClubs_Create_CreatesClubAndRedirectsToIndex()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Clubs/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Clubs/Create",
            new Dictionary<string, string>
            {
                ["Input.Name"] = "Spark Odesa",
                ["Input.City"] = "Одеса",
                ["Input.Address"] = "вул. Дерибасівська, 10",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/Clubs", response.Headers.Location?.AbsolutePath);

        // Confirm it persisted
        using var scope = _factory.Services.CreateScope();
        var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
        var all = await clubs.GetAllAsync();
        Assert.Contains(all, c => c.Name == "Spark Odesa");
    }

    [Fact]
    public async Task Post_AdminClubs_Create_WithBlankName_ReturnsForm_WithError()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Clubs/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Clubs/Create",
            new Dictionary<string, string>
            {
                ["Input.Name"] = "",
                ["Input.City"] = "Одеса",
                ["Input.Address"] = "вул. Дерибасівська, 10",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Введіть назву клубу", body);
    }

    [Fact]
    public async Task Post_AdminClubs_Edit_UpdatesClub()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubId = clubs[0].Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Admin/Clubs/{clubId}/Edit");
        var post = AntiforgeryClient.BuildPost(
            $"/Admin/Clubs/{clubId}/Edit",
            new Dictionary<string, string>
            {
                ["Input.Name"] = "Energy Kyiv (оновлений)",
                ["Input.City"] = "Київ",
                ["Input.Address"] = "вул. Хрещатик, 1",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubService>();
        var updated = await service.GetByIdAsync(clubId);
        Assert.Equal("Energy Kyiv (оновлений)", updated!.Name);
    }

    [Fact]
    public async Task PostHx_AdminClubs_Delete_RemovesClub()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubId = seeded[1].Id; // delete Forge Lviv

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Clubs");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Clubs?handler=Delete&id={clubId}");
        req.Headers.Add("HX-Request", "true");
        // afCookie may be empty if the antiforgery cookie was already in the handler's
        // cookie jar from sign-in — the handler reattaches it automatically.
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

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubService>();
        var stillThere = await service.GetByIdAsync(clubId);
        Assert.Null(stillThere);
    }
}
