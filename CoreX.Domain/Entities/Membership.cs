using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreX.Domain.Entities
{
    public class Membership
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public Guid UserId { get; private set; }

        [Required]
        public Guid ClubId { get; private set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; private set; }

        public Guid? SubscriptionId { get; private set; }

        [ForeignKey(nameof(SubscriptionId))]
        public Subscription? Subscription { get; private set; }

        [Required]
        public DateTime StartTime { get; private set; }

        protected Membership() { }

        public Membership(
            Guid userId,
            Guid clubId,
            Guid? subscriptionId)
        {
            Id = Guid.NewGuid();

            UserId = userId;
            ClubId = clubId;
            SubscriptionId = subscriptionId;

            StartTime = DateTime.UtcNow;
        }
    }
}
