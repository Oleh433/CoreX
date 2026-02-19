using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public class Trainer
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public Guid ClubId { get; private set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; private set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; private set; } = default!;

        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string Specialization { get; private set; } = default!;

        [Required]
        [Range(0, 60)]
        public int ExperienceYears { get; private set; }

        [StringLength(500)]
        public string? Bio { get; private set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; private set; }

        [Phone]
        [StringLength(30)]
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

        public void Update(
            string fullName,
            string specialization,
            int experienceYears,
            string? bio,
            string? email,
            string? phone)
        {
            FullName = fullName;
            Specialization = specialization;
            ExperienceYears = experienceYears;

            Bio = bio;
            Email = email;
            Phone = phone;
        }
    }
}
