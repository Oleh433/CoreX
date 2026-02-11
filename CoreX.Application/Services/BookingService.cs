using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingResponseDto?> GetByIdAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
                return null;

            return BookingMapper.ToDto(booking);
        }

        public async Task<List<BookingResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);

            return bookings.Select(BookingMapper.ToDto).ToList();
        }

        public async Task<Guid> CreateAsync(CreateBookingDto dto)
        {
            if (dto.UserId == Guid.Empty)
                throw new ArgumentException("UserId is required.");

            if (dto.ClubId == Guid.Empty)
                throw new ArgumentException("ClubId is required.");

            var booking = new Booking(
                userId: dto.UserId,
                clubId: dto.ClubId,
                subscriptionId: dto.SubscriptionId,
                discountId: dto.DiscountId
            );

            await _bookingRepository.AddAsync(booking);

            await _unitOfWork.SaveChangesAsync();

            return booking.Id;
        }

        public async Task<bool> ConfirmAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                return false;

            if (booking.Status != BookingStatus.New)
                throw new Exception("Only NEW bookings can be confirmed.");

            booking.Confirm();

            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                return false;

            if (booking.Status == BookingStatus.Cancelled)
                return true;

            booking.Cancel();

            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
