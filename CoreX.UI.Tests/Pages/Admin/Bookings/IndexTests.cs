using System.Net;
using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Admin.Bookings;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public IndexTests(CoreXFactory factory) => _factory = factory;

    private async Task<Guid> CreateThrowawayBookingAsync(string fullName)
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        using var scope = _factory.Services.CreateScope();
        var subs = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var subList = await subs.GetByClubIdAsync(clubs[0].Id);
        var subId = subList.First().Id;
        var bookings = scope.ServiceProvider.GetRequiredService<IBookingService>();
        return await bookings.CreateAsync(null, new CreateBookingDto
        {
            ClubId = clubs[0].Id,
            SubscriptionId = subId,
            ContactFullName = fullName,
            ContactEmail = $"booking-{Guid.NewGuid():N}@x",
            ContactPhone = "+380501234567",
        });
    }

    [Fact]
    public async Task Get_AdminBookings_AsAdmin_ListsBookings()
    {
        var marker = $"Тест Бронювання {Guid.NewGuid():N}";
        await CreateThrowawayBookingAsync(marker);

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var response = await client.GetAsync("/Admin/Bookings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(marker, body);
    }

    [Fact]
    public async Task PostHx_AdminBookings_Confirm_FlipsToConfirmed()
    {
        var bookingId = await CreateThrowawayBookingAsync($"Confirm-{Guid.NewGuid():N}");

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Bookings");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Bookings?handler=Confirm&id={bookingId}");
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
        Assert.Contains("Підтверджено", body);
    }

    [Fact]
    public async Task PostHx_AdminBookings_Cancel_FlipsToCancelled()
    {
        var bookingId = await CreateThrowawayBookingAsync($"Cancel-{Guid.NewGuid():N}");

        var client = await TestUsers.SignedInAsAdminAsync(_factory);
        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Admin/Bookings");

        var req = new HttpRequestMessage(HttpMethod.Post, $"/Admin/Bookings?handler=Cancel&id={bookingId}");
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
        Assert.Contains("Скасовано", body);
    }

    [Fact]
    public async Task Get_AdminBookings_AsUser_RedirectsAway()
    {
        var email = $"user-nobookings-{Guid.NewGuid():N}@test";
        await TestUsers.CreateAsync(_factory, email, role: "User");
        var client = await TestUsers.SignedInClientAsync(_factory, email);

        var response = await client.GetAsync("/Admin/Bookings");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
