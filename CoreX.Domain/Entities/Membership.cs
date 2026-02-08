using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Membership
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

        public DateTime StartTime { get; private set; }

        protected Membership() { }

        public Membership(
            Guid userId,
            Guid clubId,
            DateOnly startTime,
            Guid? subscriptionId)
        {
            Id = Guid.NewGuid();

            UserId = userId;
            ClubId = clubId;
            SubscriptionId = subscriptionId;

            StartTime = DateTime.Now;
        }
    }
}
