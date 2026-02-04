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
        public Guid Id { get; private set; }

        public string Name { get; private set; } = default!;

        public string City { get; private set; } = default!;

        public string Address { get; private set; } = default!;

        public string? Description { get; private set; }

        public string? Phone { get; private set; }

        public string? Email { get; private set; }

        public double? Latitude { get; private set; }

        public double? Longitude { get; private set; }

        public ICollection<Trainer> Trainers { get; private set; } = new List<Trainer>();

        public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

        public ICollection<Discount> Discounts { get; private set; } = new List<Discount>();

        public ICollection<Vacancy> Vacancies { get; private set; } = new List<Vacancy>();

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
