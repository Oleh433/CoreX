using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<BookingResponseDto>>> MyBookings()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            return await _bookingService.GetByUserIdAsync(userId);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetAll([FromQuery] Guid? clubId)
        {
            if (clubId.HasValue)
                return await _bookingService.GetByClubIdAsync(clubId.Value);

            return await _bookingService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetById(Guid id)
        {
            var booking = await _bookingService.GetByIdAsync(id);

            if (booking == null)
                return NotFound();

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Owner");

            if (!isPrivileged && booking.UserId != userId)
                return Forbid();

            return booking;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateBookingDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var bookingId = await _bookingService.CreateAsync(userId, dto);

            return CreatedAtAction(nameof(GetById), new { id = bookingId }, bookingId);
        }

        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var ok = await _bookingService.ConfirmAsync(id);

            return ok ? NoContent() : NotFound();
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingDto? dto)
        {
            var ok = await _bookingService.CancelAsync(id, dto?.Reason);

            return ok ? NoContent() : NotFound();
        }
    }
}
