using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("information-materials")]
    public class InformationMaterialsController : ControllerBase
    {
        private readonly IInformationMaterialService _service;

        public InformationMaterialsController(IInformationMaterialService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<InformationMaterialResponseDto>>> GetAll()
        {
            return await _service.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InformationMaterialResponseDto>> GetById(Guid id)
        {
            var material = await _service.GetByIdAsync(id);

            if (material == null)
                return NotFound();

            return material;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateInformationMaterialDto dto)
        {
            var id = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInformationMaterialDto dto)
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
