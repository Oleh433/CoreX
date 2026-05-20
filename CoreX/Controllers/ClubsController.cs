using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("clubs")]
    public class ClubsController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubsController(IClubService clubService)
        {
            _clubService = clubService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ClubResponseDto>>> GetAll([FromQuery] string? city)
        {
            if (!string.IsNullOrWhiteSpace(city))
                return await _clubService.GetByCityAsync(city);

            return await _clubService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClubResponseDto>> GetById(Guid id)
        {
            var club = await _clubService.GetByIdAsync(id);

            if (club == null)
                return NotFound();

            return club;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateClubDto dto)
        {
            var id = await _clubService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClubDto dto)
        {
            var ok = await _clubService.UpdateAsync(id, dto);

            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _clubService.DeleteAsync(id);

            return ok ? NoContent() : NotFound();
        }
    }
}
