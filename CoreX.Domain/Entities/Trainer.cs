using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Trainer
    {
        protected Trainer() { }

        public Trainer(
            Guid clubId,
            string fullName,
            string specialization,
            int experienceYears,
            string? bio = null,
            string? email = null,
            string? phone = null,
            Schedule? schedule = null)
        {
            Id = Guid.NewGuid();

            ClubId = clubId;
            SetFullName(fullName);
            SetSpecialization(specialization);
            SetExperience(experienceYears);

            Bio = bio;
            Email = email;
            Phone = phone;
            Schedule = schedule;

            IsActive = true;
        }

        public Guid Id { get; private set; }

        public Guid ClubId { get; private set; }
        public Club? Club { get; private set; }

        public string FullName { get; private set; } = default!;
        public string Specialization { get; private set; } = default!;
        public int ExperienceYears { get; private set; }

        public string? Bio { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }

        public decimal? Rating { get; private set; }
        public bool IsActive { get; private set; }

        public Schedule? Schedule { get; private set; }

        public void UpdateProfile(string fullName, string specialization, int experienceYears, string? bio)
        {
            SetFullName(fullName);
            SetSpecialization(specialization);
            SetExperience(experienceYears);
            Bio = bio;
        }

        public void SetContacts(string? email, string? phone)
        {
            Email = email;
            Phone = phone;
        }

        public void SetSchedule(Schedule schedule)
        {
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        }

        public void SetRating(decimal rating)
        {
            if (rating < 0 || rating > 5) throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be 0..5");
            Rating = rating;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        private void SetFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName is required.", nameof(fullName));
            FullName = fullName.Trim();
        }

        private void SetSpecialization(string specialization)
        {
            if (string.IsNullOrWhiteSpace(specialization)) throw new ArgumentException("Specialization is required.", nameof(specialization));
            Specialization = specialization.Trim();
        }

        private void SetExperience(int experienceYears)
        {
            if (experienceYears < 0) throw new ArgumentOutOfRangeException(nameof(experienceYears));
            ExperienceYears = experienceYears;
        }
    }
}
