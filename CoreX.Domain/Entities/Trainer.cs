using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Trainer
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid ClubId { get; private set; }

        public Club? Club { get; private set; }

        public string FullName { get; private set; } = default!;

        public string Specialization { get; private set; } = default!;

        public int ExperienceYears { get; private set; }

        public string? Bio { get; private set; }

        public string? Email { get; private set; }

        public string? Phone { get; private set; }

        protected Trainer() { }

        public Trainer(
            Guid clubId,
            string fullName,
            string specialization,
            int experienceYears,
            string? bio = null,
            string? email = null,
            string? phone = null)
        {
            Id = Guid.NewGuid();

            ClubId = clubId;
            FullName = fullName;
            Specialization = specialization;
            ExperienceYears = experienceYears;

            Bio = bio;
            Email = email;
            Phone = phone;
        }
    }
}
