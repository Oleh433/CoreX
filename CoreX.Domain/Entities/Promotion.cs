using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Promotion
    {
        protected Promotion() { }

        public Promotion(
            Guid clubId,
            string title,
            DateTime startDate,
            DateTime endDate,
            string? description = null,
            decimal? discountPercent = null,
            Money? discountAmount = null,
            string? conditions = null)
        {
            if (endDate < startDate) throw new ArgumentException("EndDate must be >= StartDate.");

            ValidateDiscount(discountPercent, discountAmount);

            Id = Guid.NewGuid();

            ClubId = clubId;
            SetTitle(title);

            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            Conditions = conditions;

            DiscountPercent = discountPercent;
            DiscountAmount = discountAmount;

            IsActive = true;
        }

        public Guid Id { get; private set; }

        public Guid ClubId { get; private set; }
        public Club? Club { get; private set; }

        public string Title { get; private set; } = default!;
        public string? Description { get; private set; }

        public decimal? DiscountPercent { get; private set; }
        public Money? DiscountAmount { get; private set; }

        public string? Conditions { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public bool IsActive { get; private set; }

        public void Update(
            string title,
            DateTime startDate,
            DateTime endDate,
            string? description,
            decimal? discountPercent,
            Money? discountAmount,
            string? conditions)
        {
            if (endDate < startDate) throw new ArgumentException("EndDate must be >= StartDate.");
            ValidateDiscount(discountPercent, discountAmount);

            SetTitle(title);
            StartDate = startDate;
            EndDate = endDate;

            Description = description;
            DiscountPercent = discountPercent;
            DiscountAmount = discountAmount;
            Conditions = conditions;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        private void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
            Title = title.Trim();
        }

        private static void ValidateDiscount(decimal? percent, Money? amount)
        {
            if (percent is not null && amount is not null)
                throw new ArgumentException("Use either DiscountPercent or DiscountAmount, not both.");

            if (percent is not null && (percent <= 0 || percent > 100))
                throw new ArgumentOutOfRangeException(nameof(percent), "DiscountPercent must be (0..100].");
        }
    }

}
