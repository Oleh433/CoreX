namespace CoreX.Application.DTO
{
    public class MembershipResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ClubId { get; set; }

        public Guid? SubscriptionId { get; set; }

        public DateTime StartTime { get; set; }

        public string? ClubName { get; set; }

        public string? SubscriptionTitle { get; set; }
    }
}
