using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("api/discounts")]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountsController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpGet]
        public async Task<ActionResult<List<DiscountResponseDto>>> GetAll([FromQuery] bool? activeOnly)
        {
            if (activeOnly == true)
                return await _discountService.GetActiveAsync();

            return await _discountService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DiscountResponseDto>> GetById(Guid id)
        {
            var discount = await _discountService.GetByIdAsync(id);

            if (discount == null)
                return NotFound();

            return discount;
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateDiscountDto dto)
        {
            var id = await _discountService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountDto dto)
        {
            var ok = await _discountService.UpdateAsync(id, dto);

            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _discountService.DeleteAsync(id);

            return ok ? NoContent() : NotFound();
        }
    }
}
