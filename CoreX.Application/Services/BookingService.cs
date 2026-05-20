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
        private readonly IEmailSender _emailSender;

        public BookingService(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public async Task<BookingResponseDto?> GetByIdAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
                return null;

            return BookingMapper.ToDto(booking);
        }

        public async Task<List<BookingResponseDto>> GetAllAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();

            return bookings.Select(BookingMapper.ToDto).ToList();
        }

        public async Task<List<BookingResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);

            return bookings.Select(BookingMapper.ToDto).ToList();
        }

        public async Task<List<BookingResponseDto>> GetByClubIdAsync(Guid clubId)
        {
            var bookings = await _bookingRepository.GetByClubIdAsync(clubId);

            return bookings.Select(BookingMapper.ToDto).ToList();
        }

        public async Task<Guid> CreateAsync(Guid userId, CreateBookingDto dto)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required.");

            if (dto.ClubId == Guid.Empty)
                throw new ArgumentException("ClubId is required.");

            var booking = new Booking(
                userId: userId,
                clubId: dto.ClubId,
                contactFullName: dto.ContactFullName,
                contactEmail: dto.ContactEmail,
                contactPhone: dto.ContactPhone,
                subscriptionId: dto.SubscriptionId,
                discountId: dto.DiscountId
            );

            await _bookingRepository.AddAsync(booking);

            await _unitOfWork.SaveChangesAsync();

            await _emailSender.SendAsync(
                booking.ContactEmail,
                "Booking received",
                $"Hello {booking.ContactFullName}, your booking #{booking.Id} has been received and is awaiting confirmation.");

            return booking.Id;
        }

        public async Task<bool> ConfirmAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                return false;

            if (booking.Status != BookingStatus.New)
                throw new InvalidOperationException("Only NEW bookings can be confirmed.");

            booking.Confirm();

            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync();

            await _emailSender.SendAsync(
                booking.ContactEmail,
                "Booking confirmed",
                $"Hello {booking.ContactFullName}, your booking #{booking.Id} has been confirmed.");

            return true;
        }

        public async Task<bool> CancelAsync(Guid bookingId, string? reason = null)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                return false;

            if (booking.Status == BookingStatus.Cancelled)
                return true;

            booking.Cancel(reason);

            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync();

            var reasonText = string.IsNullOrWhiteSpace(reason)
                ? string.Empty
                : $" Reason: {reason}";

            await _emailSender.SendAsync(
                booking.ContactEmail,
                "Booking cancelled",
                $"Hello {booking.ContactFullName}, your booking #{booking.Id} has been cancelled.{reasonText}");

            return true;
        }
    }
}
