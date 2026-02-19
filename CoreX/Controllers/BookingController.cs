using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreX.UI.Controllers
{
    [Authorize]
    [Route("bookings")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET: /bookings/my
        [HttpGet("my")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var bookings = await _bookingService.GetByUserIdAsync(userId);

            return View(bookings);
        }

        // GET: /bookings/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var booking = await _bookingService.GetByIdAsync(id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // GET: /bookings/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /bookings/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var bookingId = await _bookingService.CreateAsync(new CreateBookingDto
            {
                ClubId = dto.ClubId,
                SubscriptionId = dto.SubscriptionId,
                DiscountId = dto.DiscountId
            });

            return RedirectToAction("Details", new { id = bookingId });
        }

        // POST: /bookings/confirm/{id}
        [HttpPost("confirm/{id}")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            await _bookingService.ConfirmAsync(id);

            return RedirectToAction("Details", new { id });
        }

        // POST: /bookings/cancel/{id}
        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _bookingService.CancelAsync(id);

            return RedirectToAction("Details", new { id });
        }
    }
}
