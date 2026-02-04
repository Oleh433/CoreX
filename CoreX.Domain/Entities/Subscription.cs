using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; private set; }

        public string Title { get; private set; } = default!;

        public string? Description { get; private set; }

        public decimal Price { get; private set; } = default!;

        public int DurationDays { get; private set; }

        public int? VisitsLimit { get; private set; }

        protected Subscription() { }

        public Subscription(
            string title,
            decimal price,
            int durationDays,
            int? visitsLimit = null,
            string? description = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Price = price;
            DurationDays = durationDays;
            VisitsLimit = visitsLimit;
            Description = description;
        }
    }
}
