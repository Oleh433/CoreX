using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Vacancies;

public class ApplyTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public ApplyTests(CoreXFactory factory) => _factory = factory;

    // Look up by title to deterministically reach the seeded vacancy in clubA
    // — GetActiveAsync() order is non-deterministic.
    private async Task<Guid> VacancyIdByTitleAsync(string title)
    {
        using var scope = _factory.Services.CreateScope();
        var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
        var list = await vacancies.GetActiveAsync();
        return list.Single(v => v.Title == title).Id;
    }

    [Fact]
    public async Task Get_Apply_AnonymousShowsEmptyForm()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await VacancyIdByTitleAsync("Тренер з йоги");
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Vacancies/{id}/Apply");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Input.FullName\"", body);
        Assert.Contains("name=\"Input.Email\"", body);
        Assert.Contains("name=\"Input.Phone\"", body);
        Assert.Contains("name=\"Input.Experience\"", body);
        Assert.Contains("__RequestVerificationToken", body);
    }

    [Fact]
    public async Task Post_Apply_Anonymous_CreatesApplication_AndRedirectsToApplied()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await VacancyIdByTitleAsync("Тренер з йоги");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Vacancies/{id}/Apply");
        var post = AntiforgeryClient.BuildPost(
            $"/Vacancies/{id}/Apply",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Анонімний Кандидат",
                ["Input.Email"] = "candidate@test",
                ["Input.Phone"] = "+380501234567",
                ["Input.Experience"] = "5 років викладання групових занять",
                ["Input.Message"] = "",
                ["Input.CVLink"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Vacancies/Applied", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Apply_Authenticated_RedirectsToApplied()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await VacancyIdByTitleAsync("Тренер з йоги");

        var email = $"applicant-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User", fullName: "Олена Кандидат");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Vacancies/{id}/Apply");
        var post = AntiforgeryClient.BuildPost(
            $"/Vacancies/{id}/Apply",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Олена Кандидат",
                ["Input.Email"] = email,
                ["Input.Phone"] = "+380507654321",
                ["Input.Experience"] = "3 роки",
                ["Input.Message"] = "",
                ["Input.CVLink"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Vacancies/Applied", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_Apply_WithMissingExperience_ReturnsForm_WithError()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var id = await VacancyIdByTitleAsync("Тренер з йоги");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Vacancies/{id}/Apply");
        var post = AntiforgeryClient.BuildPost(
            $"/Vacancies/{id}/Apply",
            new Dictionary<string, string>
            {
                ["Input.FullName"] = "Кандидат",
                ["Input.Email"] = "k@t",
                ["Input.Phone"] = "+380501234567",
                ["Input.Experience"] = "",
                ["Input.Message"] = "",
                ["Input.CVLink"] = "",
            },
            token, afCookie);

        var response = await client.SendAsync(post);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Опишіть досвід", body);
    }
}
