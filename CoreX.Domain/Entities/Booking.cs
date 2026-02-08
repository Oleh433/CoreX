using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public enum BookingStatus
    {
        New = 0,
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3
    }

    public class Booking
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public Guid ClubId { get; private set; }
        [ForeignKey("ClubId")]
        public Club? Club { get; private set; }

        public Guid? SubscriptionId { get; private set; }
        [ForeignKey("SubscriptionId")]
        public Subscription? Subscription { get; private set; }

        public BookingStatus Status { get; private set; }

        public Guid DiscountId { get; private set; }
        [ForeignKey("DiscountId")]
        public Discount Discount { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? CancelledAt { get; private set; }

        protected Booking() { }

        public Booking(
            Guid userId,
            Guid clubId,
            Guid? subscriptionId,
            Guid discountId)
        {
            Id = Guid.NewGuid();

            UserId = userId;
            ClubId = clubId;
            SubscriptionId = subscriptionId;
            DiscountId = discountId;

            Status = BookingStatus.New;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
