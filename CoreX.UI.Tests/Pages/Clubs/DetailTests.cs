using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Clubs;

public class DetailTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public DetailTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_ClubDetail_ShowsClubInfo()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Clubs/{clubs[0].Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(clubs[0].Name, body);
        Assert.Contains(clubs[0].City, body);
    }

    [Fact]
    public async Task Get_ClubDetail_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Clubs/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHx_TrainersHandler_ReturnsPartialWithTrainerName()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetHxAsync($"/Clubs/{clubs[0].Id}?handler=Trainers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<html", body); // partial, no full layout
        Assert.Contains("Ірина Швець", body);
    }

    [Fact]
    public async Task GetHx_GroupClassesHandler_ReturnsPartialWithClassType()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetHxAsync($"/Clubs/{clubs[0].Id}?handler=GroupClasses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<html", body);
        Assert.Contains("Yoga", body);
    }

    [Fact]
    public async Task GetHx_VacanciesHandler_ReturnsPartialWithVacancyTitle()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetHxAsync($"/Clubs/{clubs[0].Id}?handler=Vacancies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<html", body);
        Assert.Contains("Тренер з йоги", body);
    }

    [Fact]
    public async Task Get_TrainersHandler_WithoutHxHeader_Returns404()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        // Non-HTMX direct hit should not render a partial as a full page.
        var response = await client.GetAsync($"/Clubs/{clubs[0].Id}?handler=Trainers");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
