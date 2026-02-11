namespace CoreX.Application.DTO
{
    public class CreateMembershipDto
    {
        public Guid UserId { get; set; }

        public Guid ClubId { get; set; }

        public Guid? SubscriptionId { get; set; }
    }
}
