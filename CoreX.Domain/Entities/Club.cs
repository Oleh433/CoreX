using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Club
    {
        protected Club() { } // for EF

        public Club(
            string name,
            string city,
            Address address,
            Schedule workSchedule,
            string? description = null,
            string? phone = null,
            string? email = null)
        {
            Id = Guid.NewGuid();

            SetName(name);
            SetCity(city);

            Address = address ?? throw new ArgumentNullException(nameof(address));
            WorkSchedule = workSchedule ?? throw new ArgumentNullException(nameof(workSchedule));

            Description = description;
            Phone = phone;
            Email = email;

            IsActive = true;
        }

        public Guid Id { get; private set; }

        public string Name { get; private set; } = default!;
        public string City { get; private set; } = default!;
        public Address Address { get; private set; } = default!;
        public Schedule WorkSchedule { get; private set; } = default!;

        public string? Description { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }

        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }

        public bool IsActive { get; private set; }

        public ICollection<Trainer> Trainers { get; private set; } = new List<Trainer>();
        public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
        public ICollection<Promotion> Promotions { get; private set; } = new List<Promotion>();
        public ICollection<Vacancy> Vacancies { get; private set; } = new List<Vacancy>();

        public void UpdateInfo(string name, string city, Address address, Schedule workSchedule, string? description)
        {
            SetName(name);
            SetCity(city);

            Address = address ?? throw new ArgumentNullException(nameof(address));
            WorkSchedule = workSchedule ?? throw new ArgumentNullException(nameof(workSchedule));
            Description = description;
        }

        public void SetContacts(string? phone, string? email)
        {
            Phone = phone;
            Email = email;
        }

        public void SetLocation(double latitude, double longitude)
        {
            // Very lightweight validation
            if (latitude < -90 || latitude > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
            if (longitude < -180 || longitude > 180) throw new ArgumentOutOfRangeException(nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Club name is required.", nameof(name));
            Name = name.Trim();
        }

        private void SetCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.", nameof(city));
            City = city.Trim();
        }
    }
}
