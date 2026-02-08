using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public enum VacancyStatus
    {
        Open = 0,
        Closed = 1
    }

    public class Vacancy
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid ClubId { get; private set; }
        [ForeignKey("ClubId")]
        public Club? Club { get; private set; }

        public string Title { get; private set; } = default!;

        public string Description { get; private set; } = default!;

        public string Requirements { get; private set; } = default!;

        public decimal? Salary { get; private set; }

        public bool IsActive { get; private set; }

        public ICollection<VacancyApplication> Applications { get; private set; } = new List<VacancyApplication>();

        protected Vacancy() { }

        public Vacancy(
            Guid clubId,
            string title,
            string description,
            string requirements,
            decimal? salary = null)
        {
            Id = Guid.NewGuid();

            ClubId = clubId;
            Title = title;

            Description = string.IsNullOrWhiteSpace(description) ? throw new ArgumentException("Description is required.") : description.Trim();
            Requirements = string.IsNullOrWhiteSpace(requirements) ? throw new ArgumentException("Requirements is required.") : requirements.Trim();

            Salary = salary;

            IsActive = true;
        }
    }
}
