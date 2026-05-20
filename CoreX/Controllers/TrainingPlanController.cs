using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("training-plan")]
    public class TrainingPlanController : ControllerBase
    {
        private readonly ITrainingPlanService _service;

        public TrainingPlanController(ITrainingPlanService service)
        {
            _service = service;
        }

        [HttpPost("generate")]
        public ActionResult<TrainingPlanResponseDto> Generate([FromBody] TrainingPlanRequestDto request)
        {
            var plan = _service.Generate(request);

            return Ok(plan);
        }
    }
}
