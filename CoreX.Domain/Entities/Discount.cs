using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Discount
    {
        [Key]
        public Guid Id { get; private set; }

        public string Title { get; private set; } = default!;

        public string? Description { get; private set; }

        public decimal? DiscountPercent { get; private set; }

        public string? Conditions { get; private set; }

        public DateTime? StartDate { get; private set; }

        public DateTime? EndDate { get; private set; }

        public bool IsActive { get; private set; }

        protected Discount() { }

        public Discount(
            Guid clubId,
            string title,
            DateTime startDate,
            DateTime endDate,
            string? description = null,
            decimal? discountPercent = null,
            string? conditions = null)
        {
            if (endDate < startDate) 
                throw new ArgumentException("EndDate must be >= StartDate.");

            Id = Guid.NewGuid();

            DiscountPercent = discountPercent;
            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            Conditions = conditions;
            DiscountPercent = discountPercent;

            IsActive = true;
        }
    }

}
