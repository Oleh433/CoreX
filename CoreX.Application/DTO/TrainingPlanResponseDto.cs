namespace CoreX.Application.DTO
{
    public class TrainingPlanResponseDto
    {
        public List<TrainingSessionDto> Sessions { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();
    }

    public class TrainingSessionDto
    {
        public string Day { get; set; } = default!;

        public string Time { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        public int DurationMinutes { get; set; }
    }
}
