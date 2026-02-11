namespace CoreX.Application.DTO
{
    public class CreateBookingDto
    {
        public Guid UserId { get; set; }

        public Guid ClubId { get; set; }

        public Guid? SubscriptionId { get; set; }

        public Guid? DiscountId { get; set; }
    }
}
