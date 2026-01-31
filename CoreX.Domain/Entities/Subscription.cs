using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Subscription
    {
        protected Subscription() { } // for EF

        public Subscription(
            Guid clubId,
            string title,
            Money price,
            int durationDays,
            int? visitsLimit = null,
            string? description = null)
        {
            Id = Guid.NewGuid();

            ClubId = clubId;
            SetTitle(title);

            Price = price;
            SetDuration(durationDays);

            VisitsLimit = visitsLimit;
            Description = description;

            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public Guid ClubId { get; private set; }
        public Club? Club { get; private set; }

        public string Title { get; private set; } = default!;
        public string? Description { get; private set; }

        public Money Price { get; private set; } = default!;
        public int DurationDays { get; private set; }
        public int? VisitsLimit { get; private set; }

        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public void Update(string title, Money price, int durationDays, int? visitsLimit, string? description)
        {
            SetTitle(title);
            Price = price;
            SetDuration(durationDays);
            VisitsLimit = visitsLimit;
            Description = description;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        private void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
            Title = title.Trim();
        }

        private void SetDuration(int durationDays)
        {
            if (durationDays <= 0) throw new ArgumentOutOfRangeException(nameof(durationDays));
            DurationDays = durationDays;
        }
    }
}
