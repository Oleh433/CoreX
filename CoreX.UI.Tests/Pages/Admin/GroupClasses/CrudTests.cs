using System.Net;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.GroupClasses;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminGroupClasses_WithoutClubId_ShowsInstruction()
    {
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/GroupClasses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Оберіть клуб", body);
    }

    [Fact]
    public async Task Get_AdminGroupClasses_WithClubId_ListsClasses()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        // SeededClub order isn't guaranteed when the fixture bail-outs (re-seed call),
        // so locate clubA ("Energy Kyiv") by name — it has the seeded "Yoga" class.
        var clubAId = seeded.First(c => c.Name == "Energy Kyiv").Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync($"/Admin/GroupClasses?clubId={clubAId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Yoga", body);
    }

    [Fact]
    public async Task Post_AdminGroupClasses_Create_CreatesClass_AndRedirectsToIndex()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubAId = seeded.First(c => c.Name == "Energy Kyiv").Id;

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/GroupClasses/Create");
        var startTime = DateTime.UtcNow.AddDays(1);
        var post = AntiforgeryClient.BuildPost(
            "/Admin/GroupClasses/Create",
            new Dictionary<string, string>
            {
                ["Input.ClubId"] = clubAId.ToString(),
                ["Input.Type"] = "Pilates",
                ["Input.Audience"] = "0",
                ["Input.StartTime"] = startTime.ToString("yyyy-MM-ddTHH:mm"),
                ["Input.DurationMinutes"] = "60",
                ["Input.Capacity"] = "10",
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/GroupClasses", response.Headers.Location?.AbsolutePath);

        using var scope = _factory.Services.CreateScope();
        var classes = scope.ServiceProvider.GetRequiredService<IGroupClassService>();
        var clubClasses = await classes.GetByClubIdAsync(clubAId);
        Assert.Contains(clubClasses, c => c.Type == "Pilates");
    }

    [Fact]
    public async Task PostHx_AdminGroupClasses_Delete_RemovesClass()
    {
        var seeded = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var clubAId = seeded.First(c => c.Name == "Energy Kyiv").Id;

        // Create a dedicated throwaway class to delete, so we don't interfere with
        // the seeded "Yoga" fixture that the list test asserts on (xUnit ordering
        // within a class is non-deterministic).
        Guid classId;
        using (var scope = _factory.Services.CreateScope())
        {
            var classes = scope.ServiceProvider.GetRequiredService<IGroupClassService>();
            classId = await classes.CreateAsync(new CoreX.Application.DTO.CreateGroupClassDto
            {
                ClubId = clubAId,
                Type = "ToDelete",
                Audience = CoreX.Domain.Entities.GroupClassAudience.Adults,
                StartTime = DateTime.UtcNow.AddDays(2),
                DurationMinutes = 45,
                Capacity = 8,
            });
        }

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Admin/GroupClasses?clubId={clubAId}");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/GroupClasses?handler=Delete&id={classId}");
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
        var classesAfter = scopeAfter.ServiceProvider.GetRequiredService<IGroupClassService>();
        var stillThere = await classesAfter.GetByIdAsync(classId);
        Assert.Null(stillThere);
    }
}
