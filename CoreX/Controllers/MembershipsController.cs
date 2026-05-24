using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/memberships")]
    public class MembershipsController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<MembershipResponseDto>>> GetAll([FromQuery] Guid? clubId)
        {
            if (clubId.HasValue)
                return await _membershipService.GetByClubIdAsync(clubId.Value);

            return await _membershipService.GetAllAsync();
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<MembershipResponseDto>>> My()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            return await _membershipService.GetByUserIdAsync(userId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipResponseDto>> GetById(Guid id)
        {
            var membership = await _membershipService.GetByIdAsync(id);

            if (membership == null)
                return NotFound();

            return membership;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateMembershipDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var id = await _membershipService.CreateAsync(userId, dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _membershipService.DeleteAsync(id);

            return ok ? NoContent() : NotFound();
        }
    }
}
