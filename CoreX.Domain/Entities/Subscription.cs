using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid ClubId { get; private set; }
        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; private set; }

        public string Title { get; private set; } = default!;

        public string? Description { get; private set; }

        public decimal Price { get; private set; } = default!;

        public int DurationDays { get; private set; }

        public int? VisitsLimit { get; private set; }

        protected Subscription() { }

        public Subscription(
            string title,
            Guid clubId,
            decimal price,
            int durationDays,
            int? visitsLimit = null,
            string? description = null)
        {
            Id = Guid.NewGuid();

            ClubId = clubId;

            Title = title;
            Price = price;
            DurationDays = durationDays;
            VisitsLimit = visitsLimit;
            Description = description;
        }
    }
}
