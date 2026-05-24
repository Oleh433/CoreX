using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreX.Pages.Admin.Bookings;

public class IndexModel : PageModel
{
    private readonly IBookingService _bookings;
    public IndexModel(IBookingService bookings) => _bookings = bookings;

    public IReadOnlyList<BookingResponseDto> Bookings { get; private set; } = Array.Empty<BookingResponseDto>();

    public async Task OnGetAsync() => Bookings = await _bookings.GetAllAsync();

    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        await _bookings.ConfirmAsync(id);
        var updated = await _bookings.GetByIdAsync(id);
        if (updated is null) return NotFound();
        return Partial("_BookingRow", updated);
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        await _bookings.CancelAsync(id, reason: null);
        var updated = await _bookings.GetByIdAsync(id);
        if (updated is null) return NotFound();
        return Partial("_BookingRow", updated);
    }
}
