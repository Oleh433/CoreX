using CoreX.Domain.Entities;

namespace CoreX.Application.DTO
{
    public class UpdateGroupClassDto
    {
        public Guid? TrainerId { get; set; }

        public string Type { get; set; } = default!;

        public string? Description { get; set; }

        public GroupClassAudience Audience { get; set; }

        public DateTime StartTime { get; set; }

        public int DurationMinutes { get; set; }

        public int Capacity { get; set; }

        public decimal? Price { get; set; }
    }
}
