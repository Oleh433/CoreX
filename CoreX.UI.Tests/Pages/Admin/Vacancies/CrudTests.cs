using System.Net;
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Vacancies;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminVacancies_AsAdmin_ListsAllIncludingInactive()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Create a fresh inactive vacancy to prove the admin list shows inactive too.
        Guid inactiveId;
        Guid clubAId;
        using (var scope = _factory.Services.CreateScope())
        {
            var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
            clubAId = (await clubs.GetAllAsync()).First(c => c.Name == "Energy Kyiv").Id;

            var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
            inactiveId = await vacancies.CreateAsync(new CreateVacancyDto
            {
                ClubId = clubAId,
                Title = "Архівна вакансія",
                Description = "Колишній опис вакансії, яка вже не активна.",
                Requirements = "архів",
            });
            await vacancies.DeactivateAsync(inactiveId);
        }

        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Vacancies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Тренер з йоги", body);
        Assert.Contains("Адміністратор", body);
        Assert.Contains("Архівна вакансія", body);
        Assert.Contains("Неактивна", body);
    }

    [Fact]
    public async Task Post_AdminVacancies_Create_CreatesVacancyAndRedirects()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubAId = seeded.First(c => c.Name == "Energy Kyiv").Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Vacancies/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Vacancies/Create",
            new Dictionary<string, string>
            {
                ["Input.ClubId"] = clubAId.ToString(),
                ["Input.Title"] = "Менеджер з продажу абонементів",
                ["Input.Description"] = "Шукаємо комунікабельного менеджера з продажу клубних абонементів.",
                ["Input.Requirements"] = "досвід продажів від 1 року",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/Vacancies", response.Headers.Location?.AbsolutePath);

        using var scope = _factory.Services.CreateScope();
        var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
        var all = await vacancies.GetAllAsync();
        Assert.Contains(all, v => v.Title == "Менеджер з продажу абонементів");
    }

    [Fact]
    public async Task Post_AdminVacancies_Create_WithBlankTitle_ReturnsForm_WithError()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubAId = seeded.First(c => c.Name == "Energy Kyiv").Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Vacancies/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Vacancies/Create",
            new Dictionary<string, string>
            {
                ["Input.ClubId"] = clubAId.ToString(),
                ["Input.Title"] = "",
                ["Input.Description"] = "Опис, який цілком достатньо довгий, щоб пройти валідацію.",
                ["Input.Requirements"] = "вимоги",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Введіть заголовок", body);
    }

    [Fact]
    public async Task Post_AdminVacancies_Edit_UpdatesVacancy()
    {
        // Create a dedicated throwaway vacancy to mutate, so we don't interfere with
        // the seeded fixture other tests in this class rely on.
        Guid vacancyId;
        using (var scope = _factory.Services.CreateScope())
        {
            var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
            var clubAId = (await clubs.GetAllAsync()).First(c => c.Name == "Energy Kyiv").Id;

            var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
            vacancyId = await vacancies.CreateAsync(new CreateVacancyDto
            {
                ClubId = clubAId,
                Title = "Вакансія до редагування",
                Description = "Початковий опис, доволі довгий для проходження валідації.",
                Requirements = "початкові вимоги",
            });
        }

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Admin/Vacancies/{vacancyId}/Edit");
        var post = AntiforgeryClient.BuildPost(
            $"/Admin/Vacancies/{vacancyId}/Edit",
            new Dictionary<string, string>
            {
                ["Input.Title"] = "Оновлений заголовок вакансії",
                ["Input.Description"] = "Оновлений опис вакансії, теж довжиною понад 10 символів.",
                ["Input.Requirements"] = "оновлені вимоги",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        using var scope2 = _factory.Services.CreateScope();
        var vacanciesAfter = scope2.ServiceProvider.GetRequiredService<IVacancyService>();
        var updated = await vacanciesAfter.GetByIdAsync(vacancyId);
        Assert.NotNull(updated);
        Assert.Equal("Оновлений заголовок вакансії", updated!.Title);
    }

    [Fact]
    public async Task PostHx_AdminVacancies_Deactivate_FlipsIsActive()
    {
        // Throwaway active vacancy created in the test — avoid touching seeded data.
        Guid vacancyId;
        using (var scope = _factory.Services.CreateScope())
        {
            var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
            var clubAId = (await clubs.GetAllAsync()).First(c => c.Name == "Energy Kyiv").Id;

            var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
            vacancyId = await vacancies.CreateAsync(new CreateVacancyDto
            {
                ClubId = clubAId,
                Title = "Активна до деактивації",
                Description = "Опис вакансії, який буде деактивовано HTMX-кнопкою.",
                Requirements = "вимоги",
            });
        }

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Vacancies");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Vacancies?handler=Deactivate&id={vacancyId}");
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
        Assert.Contains("Неактивна", body);

        using var scopeAfter = _factory.Services.CreateScope();
        var vacanciesAfter = scopeAfter.ServiceProvider.GetRequiredService<IVacancyService>();
        var refetched = await vacanciesAfter.GetByIdAsync(vacancyId);
        Assert.NotNull(refetched);
        Assert.False(refetched!.IsActive);
    }

    [Fact]
    public async Task PostHx_AdminVacancies_Delete_RemovesVacancy()
    {
        // Throwaway vacancy — avoid touching seeded data.
        Guid vacancyId;
        using (var scope = _factory.Services.CreateScope())
        {
            var clubs = scope.ServiceProvider.GetRequiredService<IClubService>();
            var clubAId = (await clubs.GetAllAsync()).First(c => c.Name == "Energy Kyiv").Id;

            var vacancies = scope.ServiceProvider.GetRequiredService<IVacancyService>();
            vacancyId = await vacancies.CreateAsync(new CreateVacancyDto
            {
                ClubId = clubAId,
                Title = "Вакансія до видалення",
                Description = "Опис вакансії, яка буде видалена HTMX-кнопкою.",
                Requirements = "вимоги",
            });
        }

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Vacancies");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Vacancies?handler=Delete&id={vacancyId}");
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
        var vacanciesAfter = scopeAfter.ServiceProvider.GetRequiredService<IVacancyService>();
        var stillThere = await vacanciesAfter.GetByIdAsync(vacancyId);
        Assert.Null(stillThere);
    }
}
