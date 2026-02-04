using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Guid Id { get; private set; }

        public Guid VacancyId { get; private set; }

        public Vacancy? Vacancy { get; private set; }

        public Guid? ApplicantId { get; private set; }

        public string FullName { get; private set; } = default!;

        public string Email { get; private set; } = default!;

        public string Phone { get; private set; } = default!;

        public string? Message { get; private set; }

        public string? CVLink { get; private set; }

        public VacancyApplicationStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        protected VacancyApplication() { }

        public VacancyApplication(
            Guid vacancyId,
            string fullName,
            string email,
            string phone,
            string? message = null,
            string? cvLink = null,
            Guid? trainerId = null)
        {
            Id = Guid.NewGuid();

            VacancyId = vacancyId;

            FullName = string.IsNullOrWhiteSpace(fullName) ? throw new ArgumentException("FullName is required.") : fullName.Trim();
            Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentException("Email is required.") : email.Trim();
            Phone = string.IsNullOrWhiteSpace(phone) ? throw new ArgumentException("Phone is required.") : phone.Trim();

            Message = message;
            CVLink = cvLink;
            ApplicantId = trainerId;

            Status = VacancyApplicationStatus.New;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
