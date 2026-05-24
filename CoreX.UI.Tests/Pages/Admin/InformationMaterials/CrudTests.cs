using System.Net;
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.InformationMaterials;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminMaterials_AsAdmin_ListsMaterials()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/InformationMaterials");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Правила відвідування", body);
        Assert.Contains("Як забронювати тренера", body);
    }

    [Fact]
    public async Task Post_AdminMaterials_Create_CreatesMaterial_AndRedirects()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/InformationMaterials/Create");
        var post = AntiforgeryClient.BuildPost(
            "/Admin/InformationMaterials/Create",
            new Dictionary<string, string>
            {
                ["Input.Title"] = "Новий матеріал про харчування",
                ["Input.Body"] = "Збалансоване харчування — це основа здоров'я та якісних тренувань.",
                ["Input.Category"] = "Харчування",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/InformationMaterials", response.Headers.Location?.AbsolutePath);

        using var scope = _factory.Services.CreateScope();
        var materials = scope.ServiceProvider.GetRequiredService<IInformationMaterialService>();
        var all = await materials.GetAllAsync();
        Assert.Contains(all, m => m.Title == "Новий матеріал про харчування");
    }

    [Fact]
    public async Task PostHx_AdminMaterials_Delete_RemovesMaterial()
    {
        // Create a throwaway material; the test mutates state and shouldn't lean on shared seed.
        Guid materialId;
        using (var scope = _factory.Services.CreateScope())
        {
            var materials = scope.ServiceProvider.GetRequiredService<IInformationMaterialService>();
            materialId = await materials.CreateAsync(new CreateInformationMaterialDto
            {
                Title = "Тимчасовий матеріал для видалення",
                Body = "Цей запис створено лише для перевірки видалення через HTMX.",
                Category = "Тест",
            });
        }

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/InformationMaterials");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/InformationMaterials?handler=Delete&id={materialId}");
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
        var materialsAfter = scopeAfter.ServiceProvider.GetRequiredService<IInformationMaterialService>();
        var stillThere = await materialsAfter.GetByIdAsync(materialId);
        Assert.Null(stillThere);
    }
}
