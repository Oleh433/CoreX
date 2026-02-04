using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Domain.Entities
{
    public class Membership
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public Guid ClubId { get; private set; }

        public Club? Club { get; private set; }

        public Guid? SubscriptionId { get; private set; }

        public Subscription? Subscription { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public BookingStatus Status { get; private set; }


        protected Membership() { }

        public Membership(
            Guid userId,
            Guid clubId,
            DateOnly startTime,
            Guid? trainerId = null,
            Guid? subscriptionId = null,
            string? comment = null)
        {
            Id = Guid.NewGuid();

            UserId = userId;
            ClubId = clubId;
            SubscriptionId = subscriptionId;

            Status = BookingStatus.New;
            StartTime = DateTime.Now;
            EndTime = StartTime.AddDays(30);
        }
    }
}
