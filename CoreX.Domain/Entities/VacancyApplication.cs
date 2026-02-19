using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public enum VacancyApplicationStatus
    {
        New = 0,
        Reviewed = 1,
        Accepted = 2,
        Rejected = 3
    }

    public class VacancyApplication
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public Guid VacancyId { get; private set; }

        [ForeignKey(nameof(VacancyId))]
        public Vacancy? Vacancy { get; private set; }

        [Required]
        public Guid ApplicantId { get; private set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; private set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; private set; } = default!;

        [Required]
        [Phone]
        [StringLength(30)]
        public string Phone { get; private set; } = default!;

        [StringLength(2000)]
        public string? Message { get; private set; }

        [StringLength(500)]
        [Url]
        public string? CVLink { get; private set; }

        [Required]
        public VacancyApplicationStatus Status { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; }

        protected VacancyApplication() { }

        public VacancyApplication(
            Guid vacancyId,
            string fullName,
            string email,
            string phone,
            Guid userId,
            string? message = null,
            string? cvLink = null)
        {
            Id = Guid.NewGuid();

            VacancyId = vacancyId;

            FullName = string.IsNullOrWhiteSpace(fullName) ? throw new ArgumentException("FullName is required.") : fullName.Trim();
            Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentException("Email is required.") : email.Trim();
            Phone = string.IsNullOrWhiteSpace(phone) ? throw new ArgumentException("Phone is required.") : phone.Trim();

            Message = message;
            CVLink = cvLink;
            ApplicantId = userId;

            Status = VacancyApplicationStatus.New;
            CreatedAt = DateTime.UtcNow;
        }

        public void ChangeStatus(VacancyApplicationStatus status)
        {
            Status = status;
        }

    }
}
