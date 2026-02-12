using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class TrainerMapper
    {
        public static TrainerResponseDto ToDto(Trainer trainer)
        {
            return new TrainerResponseDto
            {
                Id = trainer.Id,
                ClubId = trainer.ClubId,
                ClubName = trainer.Club?.Name,

                FullName = trainer.FullName,
                Specialization = trainer.Specialization,
                ExperienceYears = trainer.ExperienceYears,

                Bio = trainer.Bio,
                Email = trainer.Email,
                Phone = trainer.Phone
            };
        }
    }
}
