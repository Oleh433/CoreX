using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Trainers;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminTrainers_AsAdmin_ListsTrainers()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Trainers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ірина Швець", body);
        Assert.Contains("Петро Шеремет", body);
    }

    [Fact]
    public async Task Post_AdminTrainers_Create_CreatesTrainer_AndRedirectsToIndex()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubId = seeded[0].Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Trainers/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Trainers/Create",
            new Dictionary<string, string>
            {
                ["Input.ClubId"] = clubId.ToString(),
                ["Input.FullName"] = "Олег Коваленко",
                ["Input.Specialization"] = "Бокс",
                ["Input.ExperienceYears"] = "4",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/Trainers", response.Headers.Location?.AbsolutePath);

        using var scope = _factory.Services.CreateScope();
        var trainers = scope.ServiceProvider.GetRequiredService<ITrainerService>();
        var all = await trainers.GetAllAsync();
        Assert.Contains(all, t => t.FullName == "Олег Коваленко");
    }

    [Fact]
    public async Task Post_AdminTrainers_Create_WithBlankFullName_ReturnsForm_WithError()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubId = seeded[0].Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Trainers/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Trainers/Create",
            new Dictionary<string, string>
            {
                ["Input.ClubId"] = clubId.ToString(),
                ["Input.FullName"] = "",
                ["Input.Specialization"] = "Бокс",
                ["Input.ExperienceYears"] = "4",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        // Razor HTML-encodes the apostrophe in "ім'я" → "ім&#x27;я"; assert on the safe prefix.
        Assert.Contains("Введіть повне ім", body);
    }

    [Fact]
    public async Task PostHx_AdminTrainers_Delete_RemovesTrainer()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Pick a seeded trainer to delete.
        Guid trainerId;
        using (var scope = _factory.Services.CreateScope())
        {
            var trainers = scope.ServiceProvider.GetRequiredService<ITrainerService>();
            var all = await trainers.GetAllAsync();
            trainerId = all.First(t => t.FullName == "Петро Шеремет").Id;
        }

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Trainers");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Trainers?handler=Delete&id={trainerId}");
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
        var trainersAfter = scopeAfter.ServiceProvider.GetRequiredService<ITrainerService>();
        var stillThere = await trainersAfter.GetByIdAsync(trainerId);
        Assert.Null(stillThere);
    }
}
