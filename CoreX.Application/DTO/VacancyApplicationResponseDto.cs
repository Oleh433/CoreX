namespace CoreX.Application.DTO
{
    public class VacancyApplicationResponseDto
    {
        public Guid Id { get; set; }

        public Guid VacancyId { get; set; }

        public string? VacancyTitle { get; set; }

        public Guid ApplicantId { get; set; }

        public string FullName { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string Phone { get; set; } = default!;

        public string Experience { get; set; } = default!;

        public string? Message { get; set; }

        public string? CVLink { get; set; }

        public string Status { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }
}
