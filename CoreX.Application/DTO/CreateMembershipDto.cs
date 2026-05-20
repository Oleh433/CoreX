namespace CoreX.Application.DTO
{
    public class CreateMembershipDto
    {
        public Guid ClubId { get; set; }

        public Guid? SubscriptionId { get; set; }
    }
}
