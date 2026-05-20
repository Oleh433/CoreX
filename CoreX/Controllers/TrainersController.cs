using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("trainers")]
    public class TrainersController : ControllerBase
    {
        private readonly ITrainerService _trainerService;

        public TrainersController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TrainerResponseDto>>> GetAll([FromQuery] Guid? clubId)
        {
            if (clubId.HasValue)
                return await _trainerService.GetByClubIdAsync(clubId.Value);

            return await _trainerService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrainerResponseDto>> GetById(Guid id)
        {
            var trainer = await _trainerService.GetByIdAsync(id);

            if (trainer == null)
                return NotFound();

            return trainer;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateTrainerDto dto)
        {
            var id = await _trainerService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrainerDto dto)
        {
            var ok = await _trainerService.UpdateAsync(id, dto);

            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _trainerService.DeleteAsync(id);

            return ok ? NoContent() : NotFound();
        }
    }
}
