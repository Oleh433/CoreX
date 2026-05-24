using System.Net;
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.VacancyApplications;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    private async Task<Guid> CreateApplicationAsync(string fullName = "Кандидат Тест")
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        using var scope = _factory.Services.CreateScope();
        var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
        var vList = await vacancies.GetActiveAsync();
        var vacancyId = vList.First(v => v.Title == "Тренер з йоги").Id;
        var apps = scope.ServiceProvider.GetRequiredService<IVacancyApplicationService>();
        return await apps.ApplyAsync(new CreateVacancyApplicationDto
        {
            VacancyId = vacancyId,
            FullName = fullName,
            Email = $"app-{Guid.NewGuid():N}@test",
            Phone = "+380501234567",
            Experience = "5 років досвіду",
        }, applicantId: null);
    }

    [Fact]
    public async Task Get_AdminApplications_AsAdmin_ListsApplications()
    {
        var marker = $"Кандидат Список {Guid.NewGuid():N}";
        await CreateApplicationAsync(marker);

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var response = await client.GetAsync("/Admin/VacancyApplications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(marker, body);
    }

    [Fact]
    public async Task PostHx_AdminApplications_Status_ToAccepted_RendersAcceptedBadge()
    {
        var appId = await CreateApplicationAsync($"Accept-{Guid.NewGuid():N}");

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/VacancyApplications");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/VacancyApplications?handler=Status&id={appId}&status=Accepted");
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
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Прийнята", body);
    }

    [Fact]
    public async Task PostHx_AdminApplications_Status_ToRejected_RendersRejectedBadge()
    {
        var appId = await CreateApplicationAsync($"Reject-{Guid.NewGuid():N}");

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/VacancyApplications");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/VacancyApplications?handler=Status&id={appId}&status=Rejected");
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
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Відхилена", body);
    }

    [Fact]
    public async Task Get_AdminApplications_AsUser_RedirectsAway()
    {
        var email = $"user-noapps-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Admin/VacancyApplications");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
