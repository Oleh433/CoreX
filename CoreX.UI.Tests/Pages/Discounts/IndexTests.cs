using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Discounts;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Discounts_ListsActiveOnes()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Discounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Студентам -15%", body);
        Assert.Contains("Літня акція", body);
    }
}
