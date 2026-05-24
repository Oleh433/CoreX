using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public Guid? UserId { get; private set; }

        [Required]
        public Guid ClubId { get; private set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; private set; }

        [Required]
        public Guid SubscriptionId { get; private set; }

        [ForeignKey(nameof(SubscriptionId))]
        public Subscription? Subscription { get; private set; }

        [Required]
        public BookingStatus Status { get; private set; }

        public Guid? DiscountId { get; private set; }

        [ForeignKey(nameof(DiscountId))]
        public Discount? Discount { get; private set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string ContactFullName { get; private set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string ContactEmail { get; private set; } = default!;

        [Required]
        [Phone]
        [StringLength(30)]
        public string ContactPhone { get; private set; } = default!;

        [StringLength(500)]
        public string? CancellationReason { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; }

        public DateTime? CancelledAt { get; private set; }

        protected Booking() { }

        public Booking(
            Guid? userId,
            Guid clubId,
            Guid subscriptionId,
            string contactFullName,
            string contactEmail,
            string contactPhone,
            Guid? discountId = null)
        {
            if (subscriptionId == Guid.Empty)
                throw new ArgumentException("SubscriptionId is required.");

            Id = Guid.NewGuid();

            UserId = userId;
            ClubId = clubId;
            SubscriptionId = subscriptionId;
            DiscountId = discountId;

            ContactFullName = string.IsNullOrWhiteSpace(contactFullName)
                ? throw new ArgumentException("ContactFullName is required.")
                : contactFullName.Trim();

            ContactEmail = string.IsNullOrWhiteSpace(contactEmail)
                ? throw new ArgumentException("ContactEmail is required.")
                : contactEmail.Trim();

            ContactPhone = string.IsNullOrWhiteSpace(contactPhone)
                ? throw new ArgumentException("ContactPhone is required.")
                : contactPhone.Trim();

            Status = BookingStatus.New;
            CreatedAt = DateTime.UtcNow;
        }

        public void Confirm()
        {
            if (Status != BookingStatus.New)
                throw new InvalidOperationException("Only NEW bookings can be confirmed.");

            Status = BookingStatus.Confirmed;
        }

        public void Cancel(string? reason = null)
        {
            if (Status == BookingStatus.Completed)
                throw new InvalidOperationException("Completed booking cannot be cancelled.");

            Status = BookingStatus.Cancelled;
            CancellationReason = reason?.Trim();
            CancelledAt = DateTime.UtcNow;
        }
    }
}
