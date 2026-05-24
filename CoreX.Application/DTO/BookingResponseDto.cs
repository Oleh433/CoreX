namespace CoreX.Application.DTO
{
    public class BookingResponseDto
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        public Guid ClubId { get; set; }

        public Guid SubscriptionId { get; set; }

        public Guid? DiscountId { get; set; }

        public string ContactFullName { get; set; } = default!;

        public string ContactEmail { get; set; } = default!;

        public string ContactPhone { get; set; } = default!;

        public string? CancellationReason { get; set; }

        public string Status { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }
    }
}
