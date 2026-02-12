namespace CoreX.Application.DTO
{
    public class CreateVacancyApplicationDto
    {
        public Guid VacancyId { get; set; }

        public Guid ApplicantId { get; set; }

        public string FullName { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string Phone { get; set; } = default!;

        public string? Message { get; set; }

        public string? CVLink { get; set; }
    }
}
