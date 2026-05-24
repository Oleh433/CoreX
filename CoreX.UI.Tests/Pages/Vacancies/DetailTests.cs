using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Vacancies;

public class DetailTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public DetailTests(CoreXFactory factory) => _factory = factory;

    private async Task<Guid> VacancyIdByTitleAsync(string title)
    {
        using var scope = _factory.Services.CreateScope();
        var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
        var list = await vacancies.GetActiveAsync();
        return list.Single(v => v.Title == title).Id;
    }

    [Fact]
    public async Task Get_VacancyDetail_ShowsTitleAndDescription()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await VacancyIdByTitleAsync("Тренер з йоги");
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Vacancies/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Тренер з йоги", body);
        Assert.Contains("Подати заявку", body);
    }

    [Fact]
    public async Task Get_VacancyDetail_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/Vacancies/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
