using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("vacancies")]
    public class VacanciesController : ControllerBase
    {
        private readonly IVacancyService _vacancyService;

        public VacanciesController(IVacancyService vacancyService)
        {
            _vacancyService = vacancyService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VacancyResponseDto>>> GetAll(
            [FromQuery] Guid? clubId,
            [FromQuery] bool? activeOnly)
        {
            if (clubId.HasValue)
                return await _vacancyService.GetByClubIdAsync(clubId.Value);

            if (activeOnly == true)
                return await _vacancyService.GetActiveAsync();

            return await _vacancyService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VacancyResponseDto>> GetById(Guid id)
        {
            var vacancy = await _vacancyService.GetByIdAsync(id);

            if (vacancy == null)
                return NotFound();

            return vacancy;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateVacancyDto dto)
        {
            var id = await _vacancyService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVacancyDto dto)
        {
            var ok = await _vacancyService.UpdateAsync(id, dto);

            return ok ? NoContent() : NotFound();
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var ok = await _vacancyService.ActivateAsync(id);

            return ok ? NoContent() : NotFound();
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var ok = await _vacancyService.DeactivateAsync(id);

            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _vacancyService.DeleteAsync(id);

            return ok ? NoContent() : NotFound();
        }
    }
}
