using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Memberships;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Memberships_WithClubId_ListsSubscriptions()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Memberships?clubId={clubs[0].Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Місячний", body);
        Assert.Contains("800", body); // price renders
    }

    [Fact]
    public async Task Get_Memberships_WithoutClubId_RendersInstructions()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Memberships");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Оберіть клуб", body);
    }
}
