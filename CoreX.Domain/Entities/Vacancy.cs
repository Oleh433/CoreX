using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public class Vacancy
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public Guid ClubId { get; private set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; private set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; private set; } = default!;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Description { get; private set; } = default!;

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Requirements { get; private set; } = default!;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000)]
        public decimal? Salary { get; private set; }

        [Required]
        public bool IsActive { get; private set; } = true;

        [Required]
        public DateTime CreatedAt { get; private set; }

        public DateTime? ApplicationDeadline { get; private set; }

        public ICollection<VacancyApplication> Applications { get; private set; }
            = new List<VacancyApplication>();

        protected Vacancy() { }

        public Vacancy(
            Guid clubId,
            string title,
            string description,
            string requirements,
            decimal? salary = null,
            DateTime? applicationDeadline = null)
        {
            Id = Guid.NewGuid();

            ClubId = clubId;
            Title = title;

            Description = string.IsNullOrWhiteSpace(description) ? throw new ArgumentException("Description is required.") : description.Trim();
            Requirements = string.IsNullOrWhiteSpace(requirements) ? throw new ArgumentException("Requirements is required.") : requirements.Trim();

            Salary = salary;

            IsActive = true;

            CreatedAt = DateTime.UtcNow;
            ApplicationDeadline = applicationDeadline;
        }

        public void Update(
            string title,
            string description,
            string requirements,
            decimal? salary,
            DateTime? applicationDeadline)
        {
            Title = title.Trim();

            Description = string.IsNullOrWhiteSpace(description)
                ? throw new ArgumentException("Description is required.")
                : description.Trim();

            Requirements = string.IsNullOrWhiteSpace(requirements)
                ? throw new ArgumentException("Requirements is required.")
                : requirements.Trim();

            Salary = salary;
            ApplicationDeadline = applicationDeadline;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }
}
