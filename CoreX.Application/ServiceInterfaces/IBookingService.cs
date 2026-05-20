using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> GetByIdAsync(Guid id);

        Task<List<BookingResponseDto>> GetAllAsync();

        Task<List<BookingResponseDto>> GetByUserIdAsync(Guid userId);

        Task<List<BookingResponseDto>> GetByClubIdAsync(Guid clubId);

        Task<Guid> CreateAsync(CreateBookingDto dto);

        Task<bool> ConfirmAsync(Guid bookingId);

        Task<bool> CancelAsync(Guid bookingId, string? reason = null);
    }
}
