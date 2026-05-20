using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface ITrainingPlanService
    {
        TrainingPlanResponseDto Generate(TrainingPlanRequestDto request);
    }
}
