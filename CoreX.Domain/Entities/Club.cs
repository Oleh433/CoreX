using System.ComponentModel.DataAnnotations;

namespace CoreX.Domain.Entities
{
    public class Club
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; private set; } = default!;

        [Required]
        [StringLength(50)]
        public string City { get; private set; } = default!;

        [Required]
        [StringLength(100)]
        public string Address { get; private set; } = default!;

        [StringLength(500)]
        public string? Description { get; private set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; private set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; private set; }

        [Range(-90, 90)]
        public double? Latitude { get; private set; }

        [Range(-180, 180)]
        public double? Longitude { get; private set; }

        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
        public ICollection<Trainer> Trainers { get; private set; } = new List<Trainer>();
        public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
        public ICollection<Vacancy> Vacancies { get; private set; } = new List<Vacancy>();
        public ICollection<Membership> Memberships { get; private set; } = new List<Membership>();

        protected Club() { }

        public Club(
            string name,
            string city,
            string address,
            double? latitude,
            double? longitude,
            string? description = null,
            string? phone = null,
            string? email = null)
        {
            Id = Guid.NewGuid();

            Name = name;
            City = city;
            Address = address;

            Latitude = latitude;
            Longitude = longitude;

            Description = description;
            Phone = phone;
            Email = email;
        }
    }
}
