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
                ContactFullName = booking.ContactFullName,
                ContactEmail = booking.ContactEmail,
                ContactPhone = booking.ContactPhone,
                CancellationReason = booking.CancellationReason,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt,
                CancelledAt = booking.CancelledAt
            };
        }
    }
}
