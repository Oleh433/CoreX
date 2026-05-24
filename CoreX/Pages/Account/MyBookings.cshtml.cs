using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Account;

public class MyBookingsModel : PageModel
{
    private readonly IBookingService _bookings;
    private readonly IClubService _clubs;
    private readonly ISubscriptionService _subscriptions;
    private readonly UserManager<ApplicationUser> _users;

    public MyBookingsModel(
        IBookingService bookings,
        IClubService clubs,
        ISubscriptionService subscriptions,
        UserManager<ApplicationUser> users)
    {
        _bookings = bookings;
        _clubs = clubs;
        _subscriptions = subscriptions;
        _users = users;
    }

    public IReadOnlyList<MyBookingRow> Rows { get; private set; } = Array.Empty<MyBookingRow>();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
            return RedirectToPage("/Account/Login");

        var bookings = await _bookings.GetByUserIdAsync(user.Id);

        var rows = new List<MyBookingRow>(bookings.Count);
        foreach (var b in bookings)
        {
            var club = await _clubs.GetByIdAsync(b.ClubId);
            var sub = await _subscriptions.GetByIdAsync(b.SubscriptionId);
            rows.Add(new MyBookingRow(
                b.Id,
                club?.Name ?? "—",
                sub?.Title ?? "—",
                b.Status,
                b.CreatedAt));
        }

        Rows = rows;
        return Page();
    }

    public sealed record MyBookingRow(
        Guid Id,
        string ClubName,
        string SubscriptionName,
        string Status,
        DateTime CreatedAt);
}
