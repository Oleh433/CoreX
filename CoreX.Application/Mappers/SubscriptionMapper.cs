using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class SubscriptionMapper
    {
        public static SubscriptionResponseDto ToDto(Subscription subscription)
        {
            return new SubscriptionResponseDto
            {
                Id = subscription.Id,
                ClubId = subscription.ClubId,
                Title = subscription.Title,
                Description = subscription.Description,
                Price = subscription.Price,
                DurationDays = subscription.DurationDays,
                VisitsLimit = subscription.VisitsLimit,
                IsActive = subscription.IsActive
            };
        }
    }

}
