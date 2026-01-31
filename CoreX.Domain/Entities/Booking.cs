using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected Booking() { } 

        public Booking(
            Guid userId,
            Guid clubId,
            DateTime startTime,
            DateTime endTime,
            Guid? trainerId = null,
            Guid? subscriptionId = null,
            string? comment = null)
        {
            if (endTime <= startTime) throw new ArgumentException("EndTime must be greater than StartTime.");

            Id = Guid.NewGuid();

            UserId = userId;
            ClubId = clubId;
            TrainerId = trainerId;
            SubscriptionId = subscriptionId;

            StartTime = startTime;
            EndTime = endTime;

            Comment = comment;

            Status = BookingStatus.New;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public Guid ClubId { get; private set; }
        public Club? Club { get; private set; }

        public Guid? TrainerId { get; private set; }
        public Trainer? Trainer { get; private set; }

        public Guid? SubscriptionId { get; private set; }
        public Subscription? Subscription { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public BookingStatus Status { get; private set; }

        public string? Comment { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public void Confirm()
        {
            if (Status != BookingStatus.New) throw new InvalidOperationException("Only NEW booking can be confirmed.");
            Status = BookingStatus.Confirmed;
        }

        public void Cancel(string? reason = null)
        {
            if (Status == BookingStatus.Cancelled) return;
            if (Status == BookingStatus.Completed) throw new InvalidOperationException("Completed booking cannot be cancelled.");

            Status = BookingStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(reason))
                Comment = reason;
        }

        public void Complete()
        {
            if (Status != BookingStatus.Confirmed) throw new InvalidOperationException("Only CONFIRMED booking can be completed.");
            Status = BookingStatus.Completed;
        }
    }
}
