using System.Net;
using CoreX.UI.Tests.TestSupport;
using Xunit;

namespace CoreX.UI.Tests.Pages.Vacancies;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Vacancies_ListsActiveOnes()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Vacancies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Тренер з йоги", body);
        Assert.Contains("Адміністратор", body);
    }
}
