using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("subscriptions")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SubscriptionResponseDto>>> GetAll([FromQuery] Guid? clubId)
        {
            if (clubId.HasValue)
                return await _subscriptionService.GetByClubIdAsync(clubId.Value);

            return await _subscriptionService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionResponseDto>> GetById(Guid id)
        {
            var subscription = await _subscriptionService.GetByIdAsync(id);

            if (subscription == null)
                return NotFound();

            return subscription;
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSubscriptionDto dto)
        {
            var id = await _subscriptionService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionDto dto)
        {
            var ok = await _subscriptionService.UpdateAsync(id, dto);

            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _subscriptionService.DeleteAsync(id);

            return ok ? NoContent() : NotFound();
        }
    }
}
