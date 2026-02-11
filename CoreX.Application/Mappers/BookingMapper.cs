using CoreX.Application.DTO;
using CoreX.Domain.Entities;

namespace CoreX.Application.Mappers
{
    public static class BookingMapper
    {
        public static BookingResponseDto ToDto(Booking booking)
        {
            return new BookingResponseDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                ClubId = booking.ClubId,
                SubscriptionId = booking.SubscriptionId,
                DiscountId = booking.DiscountId,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt,
                CancelledAt = booking.CancelledAt
            };
        }
    }
}
