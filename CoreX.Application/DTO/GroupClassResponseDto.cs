namespace CoreX.Application.DTO
{
    public class GroupClassResponseDto
    {
        public Guid Id { get; set; }

        public Guid ClubId { get; set; }

        public Guid? TrainerId { get; set; }

        public string? TrainerFullName { get; set; }

        public string Type { get; set; } = default!;

        public string? Description { get; set; }

        public string Audience { get; set; } = default!;

        public DateTime StartTime { get; set; }

        public int DurationMinutes { get; set; }

        public int Capacity { get; set; }

        public decimal? Price { get; set; }
    }
}
