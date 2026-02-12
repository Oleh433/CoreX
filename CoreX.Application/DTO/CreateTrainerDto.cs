namespace CoreX.Application.DTO
{
    public class CreateTrainerDto
    {
        public Guid ClubId { get; set; }

        public string FullName { get; set; } = default!;

        public string Specialization { get; set; } = default!;

        public int ExperienceYears { get; set; }

        public string? Bio { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
