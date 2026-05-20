using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public class Discount
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Title { get; private set; } = default!;

        [StringLength(500)]
        public string? Description { get; private set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercent { get; private set; }

        [StringLength(300)]
        public string? Conditions { get; private set; }

        [StringLength(40)]
        public string? PromoCode { get; private set; }

        [Required]
        public DateTime StartDate { get; private set; }

        [Required]
        public DateTime EndDate { get; private set; }

        public bool IsActive { get; private set; }

        protected Discount() { }

        public Discount(
            string title,
            DateTime startDate,
            DateTime endDate,
            string? description = null,
            decimal? discountPercent = null,
            string? conditions = null,
            string? promoCode = null)
        {
            if (endDate < startDate)
                throw new ArgumentException("EndDate must be >= StartDate.");

            Id = Guid.NewGuid();

            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            Conditions = conditions;
            DiscountPercent = discountPercent;
            PromoCode = promoCode?.Trim();

            IsActive = true;
        }
        public void Update(
            string title,
            DateTime startDate,
            DateTime endDate,
            string? description,
            decimal? discountPercent,
            string? conditions,
            string? promoCode,
            bool isActive)
        {
            if (endDate < startDate)
                throw new ArgumentException("EndDate must be >= StartDate.");

            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            DiscountPercent = discountPercent;
            Conditions = conditions;
            PromoCode = promoCode?.Trim();
            IsActive = isActive;
        }
    }

}
