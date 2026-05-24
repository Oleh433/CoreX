using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Clubs;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Clubs_ListsAllClubs()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Clubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Energy Kyiv", body);
        Assert.Contains("Forge Lviv", body);
    }

    [Fact]
    public async Task Get_Clubs_WithCityFilter_ListsOnlyMatching()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Clubs?city=%D0%9B%D1%8C%D0%B2%D1%96%D0%B2"); // "Львів"

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Forge Lviv", body);
        Assert.DoesNotContain("Energy Kyiv", body);
    }
}
