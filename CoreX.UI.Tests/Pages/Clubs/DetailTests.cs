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
}
