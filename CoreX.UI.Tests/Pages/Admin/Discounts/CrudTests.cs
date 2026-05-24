using System.Net;
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Discounts;

public class CrudTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public CrudTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_AdminDiscounts_AsAdmin_RedirectsAway()
    {
        // Admin (not Owner) must not reach the Owner-only /Admin/Discounts page.
        var client = await TestUsers.SignedInAsAdminAsync(_factory);

        var response = await client.GetAsync("/Admin/Discounts");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_AdminDiscounts_AsOwner_ListsDiscounts()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);
        var client = await TestUsers.SignedInAsOwnerAsync(_factory);

        var response = await client.GetAsync("/Admin/Discounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Студентам -15%", body);
        Assert.Contains("Літня акція", body);
    }

    [Fact]
    public async Task Post_AdminDiscounts_Create_AsOwner_CreatesDiscount()
    {
        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Discounts/Create");

        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(14);
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Discounts/Create",
            new Dictionary<string, string>
            {
                ["Input.Title"] = "Чорна п'ятниця -30%",
                ["Input.Description"] = "Знижка на всі абонементи.",
                ["Input.DiscountPercent"] = "30",
                ["Input.Conditions"] = "Тільки для нових клієнтів",
                ["Input.PromoCode"] = "BF30",
                ["Input.StartDate"] = start.ToString("yyyy-MM-ddTHH:mm"),
                ["Input.EndDate"] = end.ToString("yyyy-MM-ddTHH:mm"),
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Admin/Discounts", response.Headers.Location?.AbsolutePath);

        using var scope = _factory.Services.CreateScope();
        var discounts = scope.ServiceProvider.GetRequiredService<IDiscountService>();
        var all = await discounts.GetAllAsync();
        Assert.Contains(all, d => d.Title == "Чорна п'ятниця -30%");
    }

    [Fact]
    public async Task Post_AdminDiscounts_Create_WithEndBeforeStart_ReturnsForm_WithError()
    {
        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Discounts/Create");

        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(-1); // end before start
        var post = AntiforgeryClient.BuildPost(
            "/Admin/Discounts/Create",
            new Dictionary<string, string>
            {
                ["Input.Title"] = "Неможлива акція",
                ["Input.DiscountPercent"] = "10",
                ["Input.StartDate"] = start.ToString("yyyy-MM-ddTHH:mm"),
                ["Input.EndDate"] = end.ToString("yyyy-MM-ddTHH:mm"),
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        // Discount entity throws: "EndDate must be >= StartDate."
        Assert.Contains("EndDate", body);
    }

    [Fact]
    public async Task Post_AdminDiscounts_Edit_AsOwner_TogglesIsActive()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Throwaway active discount so we don't mutate the seeded fixture.
        Guid discountId;
        DateTime origStart;
        DateTime origEnd;
        using (var scope = _factory.Services.CreateScope())
        {
            var discounts = scope.ServiceProvider.GetRequiredService<IDiscountService>();
            origStart = DateTime.UtcNow.Date;
            origEnd = origStart.AddDays(7);
            discountId = await discounts.CreateAsync(new CreateDiscountDto
            {
                Title = "Активна до деактивації",
                DiscountPercent = 10m,
                StartDate = origStart,
                EndDate = origEnd,
            });
        }

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, $"/Admin/Discounts/{discountId}/Edit");
        var post = AntiforgeryClient.BuildPost(
            $"/Admin/Discounts/{discountId}/Edit",
            new Dictionary<string, string>
            {
                ["Input.Title"] = "Активна до деактивації",
                ["Input.DiscountPercent"] = "10",
                ["Input.StartDate"] = origStart.ToString("yyyy-MM-ddTHH:mm"),
                ["Input.EndDate"] = origEnd.ToString("yyyy-MM-ddTHH:mm"),
                // No "Input.IsActive" field — unchecked checkbox sends nothing.
            },
            token, afCookie);

        var response = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        using var scopeAfter = _factory.Services.CreateScope();
        var discountsAfter = scopeAfter.ServiceProvider.GetRequiredService<IDiscountService>();
        var refetched = await discountsAfter.GetByIdAsync(discountId);
        Assert.NotNull(refetched);
        Assert.False(refetched!.IsActive);
    }

    [Fact]
    public async Task PostHx_AdminDiscounts_Delete_AsOwner_RemovesDiscount()
    {
        await SeedData.SeedDiscoveryFixtureAsync(_factory);

        // Throwaway discount.
        Guid discountId;
        using (var scope = _factory.Services.CreateScope())
        {
            var discounts = scope.ServiceProvider.GetRequiredService<IDiscountService>();
            var start = DateTime.UtcNow.Date;
            discountId = await discounts.CreateAsync(new CreateDiscountDto
            {
                Title = "До видалення",
                DiscountPercent = 5m,
                StartDate = start,
                EndDate = start.AddDays(7),
            });
        }

        var client = await TestUsers.SignedInAsOwnerAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Discounts");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Discounts?handler=Delete&id={discountId}");
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
        var discountsAfter = scopeAfter.ServiceProvider.GetRequiredService<IDiscountService>();
        var stillThere = await discountsAfter.GetByIdAsync(discountId);
        Assert.Null(stillThere);
    }
}
