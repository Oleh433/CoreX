using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class MembershipMapper
    {
        public static MembershipResponseDto ToDto(Membership membership)
        {
            return new MembershipResponseDto
            {
                Id = membership.Id,
                UserId = membership.UserId,
                ClubId = membership.ClubId,
                SubscriptionId = membership.SubscriptionId,
                StartTime = membership.StartTime,

                ClubName = membership.Club?.Name,
                SubscriptionTitle = membership.Subscription?.Title
            };
        }
    }
}
