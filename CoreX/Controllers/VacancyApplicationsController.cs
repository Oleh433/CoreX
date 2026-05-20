using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("vacancy-applications")]
    public class VacancyApplicationsController : ControllerBase
    {
        private readonly IVacancyApplicationService _service;

        public VacancyApplicationsController(IVacancyApplicationService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<VacancyApplicationResponseDto>>> GetAll(
            [FromQuery] Guid? vacancyId)
        {
            if (vacancyId.HasValue)
                return await _service.GetByVacancyIdAsync(vacancyId.Value);

            return await _service.GetAllAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<VacancyApplicationResponseDto>> GetById(Guid id)
        {
            var app = await _service.GetByIdAsync(id);

            if (app == null)
                return NotFound();

            return app;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Apply([FromBody] CreateVacancyApplicationDto dto)
        {
            var id = await _service.ApplyAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPost("{id}/status")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeVacancyApplicationStatusDto dto)
        {
            var ok = await _service.ChangeStatusAsync(id, dto);

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
