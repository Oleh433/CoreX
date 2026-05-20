namespace CoreX.Application.DTO
{
    public class CreateBookingDto
    {
        public Guid ClubId { get; set; }

        public Guid? SubscriptionId { get; set; }

        public Guid? DiscountId { get; set; }

        public string ContactFullName { get; set; } = default!;

        public string ContactEmail { get; set; } = default!;

        public string ContactPhone { get; set; } = default!;
    }
}
