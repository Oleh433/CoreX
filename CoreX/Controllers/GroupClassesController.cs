using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("api/group-classes")]
    public class GroupClassesController : ControllerBase
    {
        private readonly IGroupClassService _service;

        public GroupClassesController(IGroupClassService service)
        {
            _service = service;
        }

        [HttpGet("by-club/{clubId}")]
        public async Task<ActionResult<List<GroupClassResponseDto>>> GetByClub(
            Guid clubId,
            [FromQuery] GroupClassAudience? audience)
        {
            return await _service.GetByClubIdAsync(clubId, audience);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupClassResponseDto>> GetById(Guid id)
        {
            var groupClass = await _service.GetByIdAsync(id);

            if (groupClass == null)
                return NotFound();

            return groupClass;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateGroupClassDto dto)
        {
            var id = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupClassDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);

            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);

            return ok ? NoContent() : NotFound();
        }
    }
}
