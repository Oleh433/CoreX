using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class GroupClassMapper
    {
        public static GroupClassResponseDto ToDto(GroupClass groupClass)
        {
            return new GroupClassResponseDto
            {
                Id = groupClass.Id,
                ClubId = groupClass.ClubId,
                TrainerId = groupClass.TrainerId,
                TrainerFullName = groupClass.Trainer?.FullName,
                Type = groupClass.Type,
                Description = groupClass.Description,
                Audience = groupClass.Audience.ToString(),
                StartTime = groupClass.StartTime,
                DurationMinutes = groupClass.DurationMinutes,
                Capacity = groupClass.Capacity,
                Price = groupClass.Price
            };
        }
    }
}
