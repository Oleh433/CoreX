using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.IdentityEntities;
using CoreX.Pages.Memberships.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Memberships;

public class BookModel : PageModel
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IBookingService _bookings;
    private readonly UserManager<ApplicationUser> _users;

    public BookModel(
        ISubscriptionService subscriptions,
        IBookingService bookings,
        UserManager<ApplicationUser> users)
    {
        _subscriptions = subscriptions;
        _bookings = bookings;
        _users = users;
    }

    public SubscriptionResponseDto Subscription { get; private set; } = default!;

    [BindProperty]
    public BookingInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid subId)
    {
        var sub = await _subscriptions.GetByIdAsync(subId);
        if (sub is null) return NotFound();
        Subscription = sub;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _users.GetUserAsync(User);
            if (user is not null)
            {
                Input.ContactFullName = user.FullName;
                Input.ContactEmail = user.Email ?? string.Empty;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid subId)
    {
        var sub = await _subscriptions.GetByIdAsync(subId);
        if (sub is null) return NotFound();
        Subscription = sub;

        if (!ModelState.IsValid)
            return Page();

        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _users.GetUserAsync(User);
            userId = user?.Id;
        }

        Guid bookingId;
        try
        {
            bookingId = await _bookings.CreateAsync(userId, new CreateBookingDto
            {
                ClubId = sub.ClubId,
                SubscriptionId = sub.Id,
                ContactFullName = Input.ContactFullName,
                ContactEmail = Input.ContactEmail,
                ContactPhone = Input.ContactPhone,
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        var absoluteUrl = Url.Page("/Memberships/Confirmed", pageHandler: null, values: new { bookingId }, protocol: Request.Scheme);
        return Redirect(absoluteUrl!);
    }
}
