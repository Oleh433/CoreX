namespace CoreX.Application.DTO
{
    public class UpdateTrainerDto
    {
        public string FullName { get; set; } = default!;

        public string Specialization { get; set; } = default!;

        public int ExperienceYears { get; set; }

        public string? Bio { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
