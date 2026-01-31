using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public enum VacancyStatus
    {
        Open = 0,
        Closed = 1
    }

    public class Vacancy
    {
        protected Vacancy() { } 

        public Vacancy(
            Guid clubId,
            string title,
            string description,
            string requirements,
            decimal? salaryFrom = null,
            decimal? salaryTo = null)
        {
            if (salaryFrom is not null && salaryFrom < 0) throw new ArgumentOutOfRangeException(nameof(salaryFrom));
            if (salaryTo is not null && salaryTo < 0) throw new ArgumentOutOfRangeException(nameof(salaryTo));
            if (salaryFrom is not null && salaryTo is not null && salaryTo < salaryFrom)
                throw new ArgumentException("SalaryTo must be >= SalaryFrom.");

            Id = Guid.NewGuid();

            ClubId = clubId;
            SetTitle(title);

            Description = string.IsNullOrWhiteSpace(description) ? throw new ArgumentException("Description is required.") : description.Trim();
            Requirements = string.IsNullOrWhiteSpace(requirements) ? throw new ArgumentException("Requirements is required.") : requirements.Trim();

            SalaryFrom = salaryFrom;
            SalaryTo = salaryTo;

            Status = VacancyStatus.Open;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public Guid ClubId { get; private set; }
        public Club? Club { get; private set; }

        public string Title { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public string Requirements { get; private set; } = default!;

        public decimal? SalaryFrom { get; private set; }
        public decimal? SalaryTo { get; private set; }

        public VacancyStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }

        public ICollection<VacancyApplication> Applications { get; private set; } = new List<VacancyApplication>();

        public void Update(string title, string description, string requirements, decimal? salaryFrom, decimal? salaryTo)
        {
            if (salaryFrom is not null && salaryFrom < 0) throw new ArgumentOutOfRangeException(nameof(salaryFrom));
            if (salaryTo is not null && salaryTo < 0) throw new ArgumentOutOfRangeException(nameof(salaryTo));
            if (salaryFrom is not null && salaryTo is not null && salaryTo < salaryFrom)
                throw new ArgumentException("SalaryTo must be >= SalaryFrom.");

            SetTitle(title);

            Description = string.IsNullOrWhiteSpace(description) ? throw new ArgumentException("Description is required.") : description.Trim();
            Requirements = string.IsNullOrWhiteSpace(requirements) ? throw new ArgumentException("Requirements is required.") : requirements.Trim();

            SalaryFrom = salaryFrom;
            SalaryTo = salaryTo;
        }

        public void Close()
        {
            if (Status == VacancyStatus.Closed) return;
            Status = VacancyStatus.Closed;
            ClosedAt = DateTime.UtcNow;
        }

        public void Reopen()
        {
            Status = VacancyStatus.Open;
            ClosedAt = null;
        }

        private void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
            Title = title.Trim();
        }
    }
}
