namespace CoreX.Application.DTO
{
    public class TrainerResponseDto
    {
        public Guid Id { get; set; }

        public Guid ClubId { get; set; }

        public string? ClubName { get; set; }

        public string FullName { get; set; } = default!;

        public string Specialization { get; set; } = default!;

        public int ExperienceYears { get; set; }

        public string? Bio { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
