using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public Guid ClubId { get; private set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; private set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; private set; } = default!;

        [StringLength(500)]
        public string? Description { get; private set; }

        [Required]
        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; private set; }

        [Required]
        [Range(1, 365)]
        public int DurationDays { get; private set; }

        [Range(1, 100)]
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
